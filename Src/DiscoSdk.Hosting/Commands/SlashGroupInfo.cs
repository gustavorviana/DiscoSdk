using DiscoSdk.Commands;
using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Hosting.Commands;

internal class SlashGroupInfo
{
    private readonly List<CommandInfo> _subcommands = [];

    public SlashCommandAttribute ParentInfo { get; }

    public SlashGroupInfo(SlashCommandAttribute parentInfo)
    {
        ParentInfo = parentInfo;
    }

    public void Add(CommandInfo command)
    {
        var duplicate = _subcommands.FirstOrDefault(c =>
            string.Equals(c.SubCommand?.Name, command.SubCommand?.Name, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(c.SubCommandGroup?.Name, command.SubCommandGroup?.Name, StringComparison.OrdinalIgnoreCase));

        if (duplicate != null)
        {
            var groupPart = command.SubCommandGroup != null ? $" in group '{command.SubCommandGroup.Name}'" : "";
            throw new InvalidOperationException(
                $"Duplicate subcommand '{command.SubCommand!.Name}'{groupPart} found in command '{ParentInfo.Name}'.");
        }

        _subcommands.Add(command);
    }

    public IReadOnlyList<CommandInfo> Subcommands => _subcommands;

    public CommandInfo? FindCommand(string? subcommandGroup, string subcommand)
    {
        return _subcommands.FirstOrDefault(c =>
            string.Equals(c.SubCommand?.Name, subcommand, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(c.SubCommandGroup?.Name, subcommandGroup, StringComparison.OrdinalIgnoreCase));
    }

    public SlashCommandBuilder GetCommandBuilder(Func<AutocompleteName, bool> hasAutocomplete)
    {
        var builder = new SlashCommandBuilder();
        builder.WithName(ParentInfo.Name);
        builder.WithDescription(ParentInfo.Description);
        builder.WithType(ApplicationCommandType.ChatInput);

        var withoutGroup = _subcommands.Where(c => c.SubCommandGroup == null);
        var withGroup = _subcommands
            .Where(c => c.SubCommandGroup != null)
            .GroupBy(c => c.SubCommandGroup!.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var cmd in withoutGroup)
        {
            builder.AddSubCommandOption(
                cmd.SubCommand!.Name,
                cmd.SubCommand.Description,
                cmd.BuildLeafOptions(hasAutocomplete));
        }

        foreach (var group in withGroup)
        {
            var groupAttr = group.First().SubCommandGroup!;
            var subcommandOptions = group.Select(cmd =>
            {
                var leafOptions = cmd.BuildLeafOptions(hasAutocomplete);
                return new SlashCommandOption
                {
                    Name = cmd.SubCommand!.Name.ToLowerInvariant(),
                    Description = cmd.SubCommand.Description,
                    Type = SlashCommandOptionType.SubCommand,
                    Options = leafOptions.Length > 0 ? leafOptions : null,
                };
            }).ToArray();

            builder.AddSubCommandGroupOption(groupAttr.Name, groupAttr.Description, subcommandOptions);
        }

        return builder;
    }
}

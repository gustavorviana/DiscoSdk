using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models.Enums;
using InteractionOptionModel = DiscoSdk.Models.InteractionOption;

namespace DiscoSdk.Hosting.Contexts;

internal class CommandContext : InteractionContextWrapper, ICommandContext
{
    public string Name { get; }
    public string? Subcommand { get; }
    public string? SubcommandGroup { get; }
    public IReadOnlyCollection<IRootCommandOption> Options { get; }

    public CommandContext(DiscordClient client, InteractionWrapper interaction)
        : base(client, interaction)
    {
        Name = interaction.Data?.Name ?? string.Empty;

        var rawOptions = interaction.Data?.Options;
        ExtractSubcommandInfo(rawOptions, out var subcommandGroup, out var subcommand, out var leafOptions);

        SubcommandGroup = subcommandGroup;
        Subcommand = subcommand;
        Options = leafOptions == null
            ? []
            : [.. leafOptions.Select(x => new RootCommandOption(x, client.ObjectConverter))];
    }

    public TValue? GetOption<TValue>(string name, TValue? @default = null) where TValue : class
        => GetOption(name)?.To(@default);

    public TValue? GetOption<TValue>(string name, TValue? @default = null) where TValue : struct
        => GetOption(name)?.To(@default);

    public IRootCommandOption? GetOption(string name)
        => Options.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    internal static void ExtractSubcommandInfo(
        InteractionOptionModel[]? options,
        out string? subcommandGroup,
        out string? subcommand,
        out InteractionOptionModel[]? leafOptions)
    {
        subcommandGroup = null;
        subcommand = null;
        leafOptions = options;

        if (options is null or { Length: 0 })
            return;

        var first = options[0];

        if (first.Type == SlashCommandOptionType.SubCommandGroup)
        {
            subcommandGroup = first.Name;
            var nested = first.Options;
            if (nested is { Length: > 0 } && nested[0].Type == SlashCommandOptionType.SubCommand)
            {
                subcommand = nested[0].Name;
                leafOptions = nested[0].Options;
            }
            else
            {
                leafOptions = nested;
            }
        }
        else if (first.Type == SlashCommandOptionType.SubCommand)
        {
            subcommand = first.Name;
            leafOptions = first.Options;
        }
    }
}

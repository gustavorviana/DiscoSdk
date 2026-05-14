using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Commands;

/// <summary>
/// Focused builder for a <see cref="SlashCommandOptionType.SubCommandGroup"/>. Discord only allows
/// <see cref="SlashCommandOptionType.SubCommand"/> as direct children of a group, so the only
/// descent operation is <c>ThenSubCommand</c>.
/// </summary>
public sealed class SlashCommandSubCommandGroupBuilder : SlashCommandFluentNode
{
    internal SlashCommandOption Option { get; }

    internal SlashCommandSubCommandGroupBuilder(SlashCommandBuilder root, SlashCommandOption option)
        : base(root)
    {
        Option = option;
    }

    /// <summary>Sets localized names for the group.</summary>
    public SlashCommandSubCommandGroupBuilder WithNameLocalizations(Dictionary<string, string> localizations)
    {
        SlashCommandBuilder.ValidateLocalizations(localizations, "name");
        Option.NameLocalizations = localizations;
        return this;
    }

    /// <summary>Sets localized descriptions for the group.</summary>
    public SlashCommandSubCommandGroupBuilder WithDescriptionLocalizations(Dictionary<string, string> localizations)
    {
        SlashCommandBuilder.ValidateLocalizations(localizations, "description");
        Option.DescriptionLocalizations = localizations;
        return this;
    }

    /// <summary>
    /// Adds a sub-command inside this group and returns a builder focused on it. Chaining
    /// <c>ThenSubCommand(name)</c> twice would descend twice, which Discord doesn't allow —
    /// for sibling sub-commands inside the same group, use the callback overload.
    /// </summary>
    public SlashCommandSubCommandBuilder ThenSubCommand(string name, string description)
    {
        SlashCommandBuilder.ValidateOptionName(name);
        SlashCommandBuilder.ValidateOptionDescription(description);

        var option = new SlashCommandOption
        {
            Name = name.ToLowerInvariant(),
            Description = description,
            Type = SlashCommandOptionType.SubCommand,
        };
        AddChild(option);
        return new SlashCommandSubCommandBuilder(Root, option);
    }

    /// <summary>
    /// Callback overload of <see cref="ThenSubCommand(string, string)"/>. Use to add multiple
    /// sibling sub-commands inside the same group:
    /// <code>
    /// .SubCommandGroup("user", "User management", g => g
    ///     .ThenSubCommand("ban", "Ban a user", ban => ban.ThenUserOption("target", "User"))
    ///     .ThenSubCommand("kick", "Kick a user", kick => kick.ThenUserOption("target", "User")))
    /// </code>
    /// </summary>
    public SlashCommandSubCommandGroupBuilder ThenSubCommand(string name, string description, Action<SlashCommandSubCommandBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(ThenSubCommand(name, description));
        return this;
    }

    private void AddChild(SlashCommandOption option)
    {
        var current = Option.Options;
        var next = current is null
            ? [option]
            : new SlashCommandOption[current.Length + 1];
        if (current is not null)
        {
            Array.Copy(current, next, current.Length);
            next[^1] = option;
        }
        if (next.Length > 25)
            throw new InvalidOperationException("A sub-command group can have a maximum of 25 sub-commands. Discord API limit.");

        Option.Options = next;
    }
}

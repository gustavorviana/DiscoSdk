namespace DiscoSdk.Commands;

/// <summary>
/// Base for every focused fluent builder produced by <see cref="SlashCommandBuilder"/>
/// (options, sub-commands, sub-command groups, choices). Carries a back-reference to the
/// root <see cref="SlashCommandBuilder"/> and re-exposes its top-level option entry points
/// so any chain can <em>reset to root</em>, EF-style.
/// <para>
/// Mirror of <see cref="DiscoSdk.Commands.Localization.OptionTranslationBuilder.Option(string)"/>:
/// calling <c>StringOption(...)</c>, <c>SubCommand(...)</c>, etc. on any focused builder
/// always adds a sibling at the command's <em>root</em> level — equivalent to EF's <c>.Include</c>
/// after <c>.ThenInclude</c>.
/// </para>
/// </summary>
public abstract class SlashCommandFluentNode
{
    internal SlashCommandBuilder Root { get; }

    private protected SlashCommandFluentNode(SlashCommandBuilder root)
    {
        Root = root;
    }

    /// <summary>Resets focus to the command root and adds a top-level string option.</summary>
    public SlashCommandStringOptionBuilder StringOption(string name, string description, bool required = false)
        => Root.StringOption(name, description, required);

    /// <summary>Resets focus to the command root and adds a top-level string option configured via callback.</summary>
    public SlashCommandBuilder StringOption(string name, string description, Action<SlashCommandStringOptionBuilder> configure)
        => Root.StringOption(name, description, configure);

    /// <summary>Resets focus to the command root and adds a top-level integer option.</summary>
    public SlashCommandIntegerOptionBuilder IntegerOption(string name, string description, bool required = false)
        => Root.IntegerOption(name, description, required);

    /// <summary>Resets focus to the command root and adds a top-level integer option configured via callback.</summary>
    public SlashCommandBuilder IntegerOption(string name, string description, Action<SlashCommandIntegerOptionBuilder> configure)
        => Root.IntegerOption(name, description, configure);

    /// <summary>Resets focus to the command root and adds a top-level number option.</summary>
    public SlashCommandNumberOptionBuilder NumberOption(string name, string description, bool required = false)
        => Root.NumberOption(name, description, required);

    /// <summary>Resets focus to the command root and adds a top-level number option configured via callback.</summary>
    public SlashCommandBuilder NumberOption(string name, string description, Action<SlashCommandNumberOptionBuilder> configure)
        => Root.NumberOption(name, description, configure);

    /// <summary>Resets focus to the command root and adds a top-level boolean option.</summary>
    public SlashCommandLeafOptionBuilder BooleanOption(string name, string description, bool required = false)
        => Root.BooleanOption(name, description, required);

    /// <summary>Resets focus to the command root and adds a top-level boolean option configured via callback.</summary>
    public SlashCommandBuilder BooleanOption(string name, string description, Action<SlashCommandLeafOptionBuilder> configure)
        => Root.BooleanOption(name, description, configure);

    /// <summary>Resets focus to the command root and adds a top-level user option.</summary>
    public SlashCommandLeafOptionBuilder UserOption(string name, string description, bool required = false)
        => Root.UserOption(name, description, required);

    /// <summary>Resets focus to the command root and adds a top-level user option configured via callback.</summary>
    public SlashCommandBuilder UserOption(string name, string description, Action<SlashCommandLeafOptionBuilder> configure)
        => Root.UserOption(name, description, configure);

    /// <summary>Resets focus to the command root and adds a top-level role option.</summary>
    public SlashCommandLeafOptionBuilder RoleOption(string name, string description, bool required = false)
        => Root.RoleOption(name, description, required);

    /// <summary>Resets focus to the command root and adds a top-level role option configured via callback.</summary>
    public SlashCommandBuilder RoleOption(string name, string description, Action<SlashCommandLeafOptionBuilder> configure)
        => Root.RoleOption(name, description, configure);

    /// <summary>Resets focus to the command root and adds a top-level channel option.</summary>
    public SlashCommandChannelOptionBuilder ChannelOption(string name, string description, bool required = false)
        => Root.ChannelOption(name, description, required);

    /// <summary>Resets focus to the command root and adds a top-level channel option configured via callback.</summary>
    public SlashCommandBuilder ChannelOption(string name, string description, Action<SlashCommandChannelOptionBuilder> configure)
        => Root.ChannelOption(name, description, configure);

    /// <summary>Resets focus to the command root and adds a top-level mentionable option.</summary>
    public SlashCommandLeafOptionBuilder MentionableOption(string name, string description, bool required = false)
        => Root.MentionableOption(name, description, required);

    /// <summary>Resets focus to the command root and adds a top-level mentionable option configured via callback.</summary>
    public SlashCommandBuilder MentionableOption(string name, string description, Action<SlashCommandLeafOptionBuilder> configure)
        => Root.MentionableOption(name, description, configure);

    /// <summary>Resets focus to the command root and adds a top-level attachment option.</summary>
    public SlashCommandLeafOptionBuilder AttachmentOption(string name, string description, bool required = false)
        => Root.AttachmentOption(name, description, required);

    /// <summary>Resets focus to the command root and adds a top-level attachment option configured via callback.</summary>
    public SlashCommandBuilder AttachmentOption(string name, string description, Action<SlashCommandLeafOptionBuilder> configure)
        => Root.AttachmentOption(name, description, configure);

    /// <summary>Resets focus to the command root and adds a top-level sub-command.</summary>
    public SlashCommandSubCommandBuilder SubCommand(string name, string description)
        => Root.SubCommand(name, description);

    /// <summary>Resets focus to the command root and adds a top-level sub-command configured via callback.</summary>
    public SlashCommandBuilder SubCommand(string name, string description, Action<SlashCommandSubCommandBuilder> configure)
        => Root.SubCommand(name, description, configure);

    /// <summary>Resets focus to the command root and adds a top-level sub-command group.</summary>
    public SlashCommandSubCommandGroupBuilder SubCommandGroup(string name, string description)
        => Root.SubCommandGroup(name, description);

    /// <summary>Resets focus to the command root and adds a top-level sub-command group configured via callback.</summary>
    public SlashCommandBuilder SubCommandGroup(string name, string description, Action<SlashCommandSubCommandGroupBuilder> configure)
        => Root.SubCommandGroup(name, description, configure);

    /// <summary>Builds the configured <see cref="DiscoSdk.Models.Commands.ApplicationCommand"/>.</summary>
    public DiscoSdk.Models.Commands.ApplicationCommand Build() => Root.Build();
}

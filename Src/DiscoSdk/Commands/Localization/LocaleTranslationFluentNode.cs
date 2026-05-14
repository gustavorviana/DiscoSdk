namespace DiscoSdk.Commands.Localization;

/// <summary>
/// Base for every focused fluent builder produced by <see cref="LocaleTranslationBuilder"/>
/// (options and choices). Carries a back-reference to the root <see cref="LocaleTranslationBuilder"/>
/// and re-exposes its top-level option entry points so any chain can <em>reset to root</em>,
/// EF-style.
/// <para>
/// Mirror of the same pattern used by the slash-command fluent API
/// (<c>SlashCommandFluentNode</c>) — calling <see cref="Option(string)"/>, <see cref="SubCommand(string)"/>
/// etc. on any focused builder always adds a sibling at the command's <em>root</em> level
/// (equivalent to EF's <c>.Include</c> after <c>.ThenInclude</c>).
/// </para>
/// </summary>
public abstract class LocaleTranslationFluentNode
{
    internal LocaleTranslationBuilder Root { get; }

    private protected LocaleTranslationFluentNode(LocaleTranslationBuilder root)
    {
        Root = root;
    }

    /// <summary>Resets focus to the locale root and adds a top-level option translation.</summary>
    public OptionTranslationBuilder Option(string optionName)
        => Root.Option(optionName);

    /// <summary>Resets focus to the locale root and adds a top-level option translation configured via callback.</summary>
    public LocaleTranslationBuilder Option(string optionName, Action<OptionTranslationBuilder> configure)
        => Root.Option(optionName, configure);

    /// <summary>Alias for <see cref="Option(string)"/> — same semantics, reads parallel to the slash-command builder.</summary>
    public OptionTranslationBuilder SubCommand(string name)
        => Root.SubCommand(name);

    /// <summary>Alias for <see cref="Option(string, Action{OptionTranslationBuilder})"/> — callback form.</summary>
    public LocaleTranslationBuilder SubCommand(string name, Action<OptionTranslationBuilder> configure)
        => Root.SubCommand(name, configure);

    /// <summary>Alias for <see cref="Option(string)"/> — same semantics, reads parallel to the slash-command builder.</summary>
    public OptionTranslationBuilder SubCommandGroup(string name)
        => Root.SubCommandGroup(name);

    /// <summary>Alias for <see cref="Option(string, Action{OptionTranslationBuilder})"/> — callback form.</summary>
    public LocaleTranslationBuilder SubCommandGroup(string name, Action<OptionTranslationBuilder> configure)
        => Root.SubCommandGroup(name, configure);

    /// <summary>
    /// Builds the <see cref="CommandLocalization"/> for the locale being configured. Available
    /// from every focused builder for parity with the slash-command builder's <c>Build()</c>.
    /// </summary>
    public CommandLocalization Build() => Root.Build();
}

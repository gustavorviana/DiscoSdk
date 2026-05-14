namespace DiscoSdk.Commands.Localization;

/// <summary>
/// Builder focused on a single option inside a <see cref="LocaleTranslationBuilder"/>.
/// Returned by <see cref="LocaleTranslationBuilder.Option(string)"/> and by
/// <see cref="ThenOption(string)"/>. Mirrors EF Core's <c>.Include</c>/<c>.ThenInclude</c>
/// chain semantics:
/// <list type="bullet">
/// <item><description><see cref="ThenOption(string)"/> descends into a child option (sub-command / sub-command group).</description></item>
/// <item><description><see cref="ThenChoice(string)"/> adds a choice to the focused option; another <c>ThenChoice</c> adds a sibling.</description></item>
/// <item><description><see cref="LocaleTranslationFluentNode.Option(string)"/> always resets focus back to the command root and adds a top-level sibling option.</description></item>
/// </list>
/// Aliases like <see cref="ThenSubCommand(string)"/> and <see cref="ThenSubCommandGroup(string)"/>
/// are provided so the translation tree reads parallel to the slash-command builder.
/// </summary>
public sealed class OptionTranslationBuilder : LocaleTranslationFluentNode
{
    private readonly OptionScratch _focused;

    internal OptionTranslationBuilder(LocaleTranslationBuilder root, OptionScratch focused)
        : base(root)
    {
        _focused = focused;
    }

    /// <summary>
    /// Sets the localized name for the focused option. Same rules as the base option name:
    /// 1-32 chars, lowercase Unicode letters / digits / hyphens / underscores.
    /// </summary>
    public OptionTranslationBuilder WithName(string name)
    {
        SlashCommandBuilder.ValidateOptionName(name);
        _focused.Name = name;
        return this;
    }

    /// <summary>Sets the localized description for the focused option (1-100 chars).</summary>
    public OptionTranslationBuilder WithDescription(string description)
    {
        SlashCommandBuilder.ValidateOptionDescription(description);
        _focused.Description = description;
        return this;
    }

    /// <summary>
    /// Adds a child option to the focused option and returns a builder focused on the child.
    /// Use for sub-commands inside a sub-command group, or for any nested option that has its
    /// own translations.
    /// </summary>
    public OptionTranslationBuilder ThenOption(string optionName)
    {
        var scratch = AddChildScratch(optionName);
        return new OptionTranslationBuilder(Root, scratch);
    }

    /// <summary>
    /// Adds a child option configured via callback. Use this when a sub-command group has
    /// multiple sub-command siblings — the callback's focus stays scoped to the child, so
    /// each sibling gets its own configure block.
    /// </summary>
    public OptionTranslationBuilder ThenOption(string optionName, Action<OptionTranslationBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var scratch = AddChildScratch(optionName);
        configure(new OptionTranslationBuilder(Root, scratch));
        return this;
    }

    /// <summary>Alias for <see cref="ThenOption(string)"/> — reads parallel to <c>SubCommandBuilder.ThenSubCommand</c> would, if it existed in the option tree.</summary>
    public OptionTranslationBuilder ThenSubCommand(string name) => ThenOption(name);

    /// <summary>Alias for <see cref="ThenOption(string, Action{OptionTranslationBuilder})"/>.</summary>
    public OptionTranslationBuilder ThenSubCommand(string name, Action<OptionTranslationBuilder> configure)
        => ThenOption(name, configure);

    /// <summary>Alias for <see cref="ThenOption(string)"/> — reads parallel to a sub-command group descent.</summary>
    public OptionTranslationBuilder ThenSubCommandGroup(string name) => ThenOption(name);

    /// <summary>Alias for <see cref="ThenOption(string, Action{OptionTranslationBuilder})"/>.</summary>
    public OptionTranslationBuilder ThenSubCommandGroup(string name, Action<OptionTranslationBuilder> configure)
        => ThenOption(name, configure);

    /// <summary>
    /// Adds a choice to the focused option and returns a builder focused on the choice.
    /// Calling <see cref="ChoiceTranslationBuilder.ThenChoice(string)"/> on the returned
    /// builder adds a sibling choice on the same option.
    /// </summary>
    public ChoiceTranslationBuilder ThenChoice(string choiceName)
    {
        var scratch = AddChoiceScratch(choiceName);
        return new ChoiceTranslationBuilder(Root, _focused, scratch);
    }

    /// <summary>
    /// Single-call overload that adds a choice and sets its localized display name in one step
    /// — sugar for <c>ThenChoice(choiceName).WithName(localizedName)</c>. Mirrors the
    /// slash-command builder's <c>ThenChoice(name, value)</c> two-argument shape.
    /// </summary>
    public ChoiceTranslationBuilder ThenChoice(string choiceName, string localizedName)
        => ThenChoice(choiceName).WithName(localizedName);

    private OptionScratch AddChildScratch(string optionName)
    {
        if (string.IsNullOrWhiteSpace(optionName))
            throw new ArgumentException("Option name cannot be null or empty.", nameof(optionName));

        var scratch = new OptionScratch(optionName);
        _focused.Options.Add(scratch);
        return scratch;
    }

    private ChoiceScratch AddChoiceScratch(string choiceName)
    {
        if (string.IsNullOrWhiteSpace(choiceName))
            throw new ArgumentException("Choice name cannot be null or empty.", nameof(choiceName));

        var scratch = new ChoiceScratch(choiceName);
        _focused.Choices.Add(scratch);
        return scratch;
    }
}

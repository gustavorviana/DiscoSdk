namespace DiscoSdk.Commands.Localization;

/// <summary>
/// Builds the translation tree for a command in a single locale. Receives the <c>t</c>
/// parameter inside <see cref="CommandLocalizationBuilder.ForLocale"/>:
/// <code>
/// .ForLocale("en-GB", t => t
///     .WithName("ban")
///     .WithDescription("Ban a user")
///     .Option("user").WithName("target")
///     .Option("reason").WithName("cause")
///         .ThenChoice("spam").WithName("Spam"))
/// </code>
/// Calling <see cref="Option(string)"/> on a focused option or choice builder also lands here
/// — it always adds a sibling at the command's <em>root</em> level (EF <c>.Include</c> reset).
/// </summary>
public sealed class LocaleTranslationBuilder
{
    internal string? Name { get; private set; }
    internal string? Description { get; private set; }
    internal List<OptionScratch> Options { get; } = [];

    /// <summary>
    /// Sets the localized command name. Same rules as <see cref="SlashCommandBuilder.WithName(string)"/>:
    /// 1-32 chars, must match <c>^[a-z0-9-_]+$</c> — Discord rejects localized names that don't
    /// satisfy the base-name regex.
    /// </summary>
    public LocaleTranslationBuilder WithName(string name)
    {
        SlashCommandBuilder.ValidateCommandName(name);
        Name = name;
        return this;
    }

    /// <summary>Sets the localized command description (1-100 chars).</summary>
    public LocaleTranslationBuilder WithDescription(string description)
    {
        SlashCommandBuilder.ValidateCommandDescription(description);
        Description = description;
        return this;
    }

    /// <summary>
    /// Adds a top-level command option and returns a builder focused on it. Calling
    /// <c>.Option(...)</c> again on the returned focus (or after a <c>.ThenChoice(...)</c>
    /// chain) resets back to this root — same semantics as EF's <c>.Include</c> after
    /// <c>.ThenInclude</c>.
    /// </summary>
    public OptionTranslationBuilder Option(string optionName)
    {
        var scratch = AddOptionScratch(optionName);
        return new OptionTranslationBuilder(this, scratch);
    }

    /// <summary>
    /// Adds a top-level option configured via a callback — convenient for sub-command groups
    /// that need multiple subcommand siblings. The callback receives a builder focused on the
    /// new option; chaining inside it stays scoped to that option.
    /// </summary>
    public LocaleTranslationBuilder Option(string optionName, Action<OptionTranslationBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var scratch = AddOptionScratch(optionName);
        configure(new OptionTranslationBuilder(this, scratch));
        return this;
    }

    /// <summary>Alias for <see cref="Option(string)"/> — reads parallel to <c>SlashCommandBuilder.SubCommand</c>.</summary>
    public OptionTranslationBuilder SubCommand(string name) => Option(name);

    /// <summary>Alias for <see cref="Option(string, Action{OptionTranslationBuilder})"/>.</summary>
    public LocaleTranslationBuilder SubCommand(string name, Action<OptionTranslationBuilder> configure)
        => Option(name, configure);

    /// <summary>Alias for <see cref="Option(string)"/> — reads parallel to <c>SlashCommandBuilder.SubCommandGroup</c>.</summary>
    public OptionTranslationBuilder SubCommandGroup(string name) => Option(name);

    /// <summary>Alias for <see cref="Option(string, Action{OptionTranslationBuilder})"/>.</summary>
    public LocaleTranslationBuilder SubCommandGroup(string name, Action<OptionTranslationBuilder> configure)
        => Option(name, configure);

    internal OptionScratch AddOptionScratch(string optionName)
    {
        if (string.IsNullOrWhiteSpace(optionName))
            throw new ArgumentException("Option name cannot be null or empty.", nameof(optionName));

        var scratch = new OptionScratch(optionName);
        Options.Add(scratch);
        return scratch;
    }

    /// <summary>
    /// Builds the <see cref="CommandLocalization"/> for this locale's translation tree. Mirrors
    /// the slash-command builder's <c>Build()</c>; <see cref="CommandLocalizationBuilder"/> calls
    /// this once per locale to assemble the final <c>(locale → CommandLocalization)</c> dictionary.
    /// </summary>
    public CommandLocalization Build() => new()
    {
        Name = Name,
        Description = Description,
        Options = Options.Count > 0 ? [.. Options.Select(o => o.Build())] : null,
    };
}

/// <summary>
/// Mutable scratch used while building an <see cref="OptionLocalization"/> — the public
/// localization records are init-only, so the builder accumulates state here then materialises
/// the immutable tree on <c>Build()</c>.
/// </summary>
internal sealed class OptionScratch(string optionName)
{
    public string OptionName { get; } = optionName;
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<OptionScratch> Options { get; } = [];
    public List<ChoiceScratch> Choices { get; } = [];

    public OptionLocalization Build() => new()
    {
        OptionName = OptionName,
        Name = Name,
        Description = Description,
        Options = Options.Count > 0 ? [.. Options.Select(o => o.Build())] : null,
        Choices = Choices.Count > 0 ? [.. Choices.Select(c => c.Build())] : null,
    };
}

internal sealed class ChoiceScratch(string choiceName)
{
    public string ChoiceName { get; } = choiceName;
    public string? Name { get; set; }

    public ChoiceLocalization Build() => new()
    {
        ChoiceName = ChoiceName,
        Name = Name,
    };
}

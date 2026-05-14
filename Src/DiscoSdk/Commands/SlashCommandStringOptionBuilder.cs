using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Commands;

/// <summary>
/// Focused builder for a <see cref="SlashCommandOptionType.String"/> option. Adds string-specific
/// constraints (min/max length, autocomplete) and a fluent <c>ThenChoice</c> chain for choices.
/// </summary>
public sealed class SlashCommandStringOptionBuilder : SlashCommandOptionNode
{
    internal SlashCommandOption Option { get; }

    internal SlashCommandStringOptionBuilder(SlashCommandBuilder root, SlashCommandOption option, ISlashCommandOptionContainer container)
        : base(root, container)
    {
        Option = option;
    }

    /// <summary>Marks the option as required (default <c>true</c>).</summary>
    public SlashCommandStringOptionBuilder WithRequired(bool required = true)
    {
        Option.Required = required;
        return this;
    }

    /// <summary>Sets the minimum length (0-6000).</summary>
    public SlashCommandStringOptionBuilder WithMinLength(int minLength)
    {
        SlashCommandBuilder.ValidateStringLengthBounds(minLength, Option.MaxLength);
        Option.MinLength = minLength;
        return this;
    }

    /// <summary>Sets the maximum length (0-6000).</summary>
    public SlashCommandStringOptionBuilder WithMaxLength(int maxLength)
    {
        SlashCommandBuilder.ValidateStringLengthBounds(Option.MinLength, maxLength);
        Option.MaxLength = maxLength;
        return this;
    }

    /// <summary>Enables autocomplete. Mutually exclusive with choices.</summary>
    public SlashCommandStringOptionBuilder WithAutocomplete(bool autocomplete = true)
    {
        if (autocomplete && Option.Choices is { Length: > 0 })
            throw new InvalidOperationException("Cannot enable autocomplete on an option that already has choices.");

        Option.Autocomplete = autocomplete;
        return this;
    }

    /// <summary>Sets localized names for the option.</summary>
    public SlashCommandStringOptionBuilder WithNameLocalizations(Dictionary<string, string> localizations)
    {
        SlashCommandBuilder.ValidateLocalizations(localizations, "name");
        Option.NameLocalizations = localizations;
        return this;
    }

    /// <summary>Sets localized descriptions for the option.</summary>
    public SlashCommandStringOptionBuilder WithDescriptionLocalizations(Dictionary<string, string> localizations)
    {
        SlashCommandBuilder.ValidateLocalizations(localizations, "description");
        Option.DescriptionLocalizations = localizations;
        return this;
    }

    /// <summary>
    /// Adds a string choice and returns a builder focused on the choice. Calling
    /// <see cref="SlashCommandChoiceBuilder{TValue}.ThenChoice(string, TValue)"/> on the returned
    /// builder adds a sibling choice on the same option.
    /// </summary>
    public SlashCommandChoiceBuilder<string> ThenChoice(string name, string value)
    {
        if (Option.Autocomplete == true)
            throw new InvalidOperationException("Cannot add choices to an option that has autocomplete enabled.");

        var choice = new SlashCommandOptionChoice { Name = name, Value = value };
        var choices = AppendChoice(choice);
        SlashCommandBuilder.ValidateChoices(choices, SlashCommandOptionType.String);
        return new SlashCommandChoiceBuilder<string>(Root, Option, choice, Container);
    }

    /// <summary>
    /// Callback overload of <see cref="ThenChoice(string, string)"/>. Use when you want to
    /// configure choice-level extras (localizations) without breaking the option chain — keeps
    /// focus on this option after the callback completes.
    /// </summary>
    public SlashCommandStringOptionBuilder ThenChoice(string name, string value, Action<SlashCommandChoiceBuilder<string>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var choiceBuilder = ThenChoice(name, value);
        configure(choiceBuilder);
        return this;
    }

    private SlashCommandOptionChoice[] AppendChoice(SlashCommandOptionChoice choice)
    {
        var current = Option.Choices;
        var next = current is null
            ? [choice]
            : new SlashCommandOptionChoice[current.Length + 1];
        if (current is not null)
        {
            Array.Copy(current, next, current.Length);
            next[^1] = choice;
        }
        Option.Choices = next;
        return next;
    }
}

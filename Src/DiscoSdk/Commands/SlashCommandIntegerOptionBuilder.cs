using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Commands;

/// <summary>
/// Focused builder for a <see cref="SlashCommandOptionType.Integer"/> option. Adds integer-specific
/// constraints (min/max value, autocomplete) and a fluent <c>ThenChoice</c> chain for choices.
/// </summary>
public sealed class SlashCommandIntegerOptionBuilder : SlashCommandOptionNode
{
    internal SlashCommandOption Option { get; }

    internal SlashCommandIntegerOptionBuilder(SlashCommandBuilder root, SlashCommandOption option, ISlashCommandOptionContainer container)
        : base(root, container)
    {
        Option = option;
    }

    /// <summary>Marks the option as required (default <c>true</c>).</summary>
    public SlashCommandIntegerOptionBuilder WithRequired(bool required = true)
    {
        Option.Required = required;
        return this;
    }

    /// <summary>Sets the minimum integer value.</summary>
    public SlashCommandIntegerOptionBuilder WithMinValue(long minValue)
    {
        SlashCommandBuilder.ValidateNumericBounds(minValue, Option.MaxValue, SlashCommandOptionType.Integer);
        Option.MinValue = minValue;
        return this;
    }

    /// <summary>Sets the maximum integer value.</summary>
    public SlashCommandIntegerOptionBuilder WithMaxValue(long maxValue)
    {
        SlashCommandBuilder.ValidateNumericBounds(Option.MinValue, maxValue, SlashCommandOptionType.Integer);
        Option.MaxValue = maxValue;
        return this;
    }

    /// <summary>Enables autocomplete. Mutually exclusive with choices.</summary>
    public SlashCommandIntegerOptionBuilder WithAutocomplete(bool autocomplete = true)
    {
        if (autocomplete && Option.Choices is { Length: > 0 })
            throw new InvalidOperationException("Cannot enable autocomplete on an option that already has choices.");

        Option.Autocomplete = autocomplete;
        return this;
    }

    /// <summary>Sets localized names for the option.</summary>
    public SlashCommandIntegerOptionBuilder WithNameLocalizations(Dictionary<string, string> localizations)
    {
        SlashCommandBuilder.ValidateLocalizations(localizations, "name");
        Option.NameLocalizations = localizations;
        return this;
    }

    /// <summary>Sets localized descriptions for the option.</summary>
    public SlashCommandIntegerOptionBuilder WithDescriptionLocalizations(Dictionary<string, string> localizations)
    {
        SlashCommandBuilder.ValidateLocalizations(localizations, "description");
        Option.DescriptionLocalizations = localizations;
        return this;
    }

    /// <summary>
    /// Adds an integer choice and returns a builder focused on the choice.
    /// </summary>
    public SlashCommandChoiceBuilder<long> ThenChoice(string name, long value)
    {
        if (Option.Autocomplete == true)
            throw new InvalidOperationException("Cannot add choices to an option that has autocomplete enabled.");

        var choice = new SlashCommandOptionChoice { Name = name, Value = value };
        var choices = AppendChoice(choice);
        SlashCommandBuilder.ValidateChoices(choices, SlashCommandOptionType.Integer);
        return new SlashCommandChoiceBuilder<long>(Root, Option, choice, Container);
    }

    /// <summary>Callback overload of <see cref="ThenChoice(string, long)"/>.</summary>
    public SlashCommandIntegerOptionBuilder ThenChoice(string name, long value, Action<SlashCommandChoiceBuilder<long>> configure)
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

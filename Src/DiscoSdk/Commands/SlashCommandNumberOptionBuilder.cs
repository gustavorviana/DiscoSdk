using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Commands;

/// <summary>
/// Focused builder for a <see cref="SlashCommandOptionType.Number"/> option (floating point).
/// Adds number-specific constraints (min/max value, autocomplete) and a fluent <c>ThenChoice</c>
/// chain for choices.
/// </summary>
public sealed class SlashCommandNumberOptionBuilder : SlashCommandOptionNode
{
    internal SlashCommandOption Option { get; }

    internal SlashCommandNumberOptionBuilder(SlashCommandBuilder root, SlashCommandOption option, ISlashCommandOptionContainer container)
        : base(root, container)
    {
        Option = option;
    }

    /// <summary>Marks the option as required (default <c>true</c>).</summary>
    public SlashCommandNumberOptionBuilder WithRequired(bool required = true)
    {
        Option.Required = required;
        return this;
    }

    /// <summary>Sets the minimum numeric value.</summary>
    public SlashCommandNumberOptionBuilder WithMinValue(double minValue)
    {
        SlashCommandBuilder.ValidateNumericBounds(minValue, Option.MaxValue, SlashCommandOptionType.Number);
        Option.MinValue = minValue;
        return this;
    }

    /// <summary>Sets the maximum numeric value.</summary>
    public SlashCommandNumberOptionBuilder WithMaxValue(double maxValue)
    {
        SlashCommandBuilder.ValidateNumericBounds(Option.MinValue, maxValue, SlashCommandOptionType.Number);
        Option.MaxValue = maxValue;
        return this;
    }

    /// <summary>Enables autocomplete. Mutually exclusive with choices.</summary>
    public SlashCommandNumberOptionBuilder WithAutocomplete(bool autocomplete = true)
    {
        if (autocomplete && Option.Choices is { Length: > 0 })
            throw new InvalidOperationException("Cannot enable autocomplete on an option that already has choices.");

        Option.Autocomplete = autocomplete;
        return this;
    }

    /// <summary>Sets localized names for the option.</summary>
    public SlashCommandNumberOptionBuilder WithNameLocalizations(Dictionary<string, string> localizations)
    {
        SlashCommandBuilder.ValidateLocalizations(localizations, "name");
        Option.NameLocalizations = localizations;
        return this;
    }

    /// <summary>Sets localized descriptions for the option.</summary>
    public SlashCommandNumberOptionBuilder WithDescriptionLocalizations(Dictionary<string, string> localizations)
    {
        SlashCommandBuilder.ValidateLocalizations(localizations, "description");
        Option.DescriptionLocalizations = localizations;
        return this;
    }

    /// <summary>
    /// Adds a numeric choice and returns a builder focused on the choice.
    /// </summary>
    public SlashCommandChoiceBuilder<double> ThenChoice(string name, double value)
    {
        if (Option.Autocomplete == true)
            throw new InvalidOperationException("Cannot add choices to an option that has autocomplete enabled.");

        var choice = new SlashCommandOptionChoice { Name = name, Value = value };
        var choices = AppendChoice(choice);
        SlashCommandBuilder.ValidateChoices(choices, SlashCommandOptionType.Number);
        return new SlashCommandChoiceBuilder<double>(Root, Option, choice, Container);
    }

    /// <summary>Callback overload of <see cref="ThenChoice(string, double)"/>.</summary>
    public SlashCommandNumberOptionBuilder ThenChoice(string name, double value, Action<SlashCommandChoiceBuilder<double>> configure)
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

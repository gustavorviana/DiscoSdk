namespace DiscoSdk.Commands.Localization;

/// <summary>
/// Builder focused on a single choice. Returned by
/// <see cref="OptionTranslationBuilder.ThenChoice(string)"/>. Choices are leaves (no children
/// to descend into), so the only operations are: translate the focused choice, add a sibling
/// choice on the same option, or reset focus back to the command root (inherited from
/// <see cref="LocaleTranslationFluentNode"/>).
/// </summary>
public sealed class ChoiceTranslationBuilder : LocaleTranslationFluentNode
{
    private const int MaxChoiceNameLen = 100;

    private readonly OptionScratch _parent;
    private readonly ChoiceScratch _focused;

    internal ChoiceTranslationBuilder(LocaleTranslationBuilder root, OptionScratch parent, ChoiceScratch focused)
        : base(root)
    {
        _parent = parent;
        _focused = focused;
    }

    /// <summary>Sets the localized name for the focused choice (1-100 chars, free-form text).</summary>
    public ChoiceTranslationBuilder WithName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        if (name.Length > MaxChoiceNameLen)
            throw new ArgumentException($"Name cannot exceed {MaxChoiceNameLen} characters.", nameof(name));

        _focused.Name = name;
        return this;
    }

    /// <summary>
    /// Adds a sibling choice on the same option and shifts focus to the new choice. Use to
    /// translate several choices on the same option in a single chain.
    /// </summary>
    public ChoiceTranslationBuilder ThenChoice(string choiceName)
    {
        if (string.IsNullOrWhiteSpace(choiceName))
            throw new ArgumentException("Choice name cannot be null or empty.", nameof(choiceName));

        var scratch = new ChoiceScratch(choiceName);
        _parent.Choices.Add(scratch);
        return new ChoiceTranslationBuilder(Root, _parent, scratch);
    }

    /// <summary>
    /// Single-call overload that adds a sibling choice and sets its localized name in one step
    /// — sugar for <c>ThenChoice(choiceName).WithName(localizedName)</c>.
    /// </summary>
    public ChoiceTranslationBuilder ThenChoice(string choiceName, string localizedName)
        => ThenChoice(choiceName).WithName(localizedName);
}

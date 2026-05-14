using DiscoSdk.Models.Commands;

namespace DiscoSdk.Commands;

/// <summary>
/// Focused builder for a single choice on a string / integer / number option. Choices are leaves
/// in the option tree, so the only descent operation is adding a sibling choice via
/// <see cref="ThenChoice(string, TValue)"/>; the value type is constrained at compile time by
/// <typeparamref name="TValue"/>.
/// </summary>
/// <typeparam name="TValue">
/// The value type for choices on the parent option (<see cref="string"/>, <see cref="long"/>, or
/// <see cref="double"/>). Picked by the parent option builder so callers can't mix value types
/// across choices on the same option.
/// </typeparam>
public sealed class SlashCommandChoiceBuilder<TValue> : SlashCommandFluentNode
{
    private readonly SlashCommandOption _parent;
    private readonly SlashCommandOptionChoice _focused;
    private readonly ISlashCommandOptionContainer _parentContainer;

    internal SlashCommandChoiceBuilder(
        SlashCommandBuilder root,
        SlashCommandOption parent,
        SlashCommandOptionChoice focused,
        ISlashCommandOptionContainer parentContainer)
        : base(root)
    {
        _parent = parent;
        _focused = focused;
        _parentContainer = parentContainer;
    }

    /// <summary>
    /// The container that owns the parent option (the one carrying this choice). Threaded through
    /// so a sibling choice keeps pointing at the same container — currently only used to preserve
    /// the chain identity; <see cref="ThenChoice(string, TValue)"/> mutates the parent option's
    /// <see cref="SlashCommandOption.Choices"/> directly.
    /// </summary>
    internal ISlashCommandOptionContainer ParentContainer => _parentContainer;

    /// <summary>
    /// Sets localized names for the focused choice. Choice names follow the choice rules
    /// (1-100 chars, free-form text) — they're <em>not</em> subject to the lowercase regex
    /// applied to command/option names.
    /// </summary>
    public SlashCommandChoiceBuilder<TValue> WithNameLocalizations(Dictionary<string, string> localizations)
    {
        ArgumentNullException.ThrowIfNull(localizations);
        foreach (var (locale, value) in localizations)
            ValidateChoiceLocalization(locale, value);

        _focused.NameLocalizations = localizations;
        return this;
    }

    /// <summary>Adds or replaces a single localized name entry on the focused choice.</summary>
    public SlashCommandChoiceBuilder<TValue> AddNameLocalization(string locale, string name)
    {
        ValidateChoiceLocalization(locale, name);
        _focused.NameLocalizations ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        _focused.NameLocalizations[locale] = name;
        return this;
    }

    private static void ValidateChoiceLocalization(string locale, string value)
    {
        SlashCommandBuilder.EnsureLocale(locale);
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"Choice localization value for locale '{locale}' cannot be null, empty, or whitespace.", nameof(value));
        var trimmed = value.Trim();
        if (trimmed.Length is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(value), $"Choice localization value for locale '{locale}' must be between 1 and 100 characters. Current length: {trimmed.Length}.");
    }

    /// <summary>
    /// Adds a sibling choice on the same parent option and shifts focus to the new choice.
    /// </summary>
    public SlashCommandChoiceBuilder<TValue> ThenChoice(string name, TValue value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var choice = new SlashCommandOptionChoice { Name = name, Value = value };
        var current = _parent.Choices;
        var next = current is null
            ? [choice]
            : new SlashCommandOptionChoice[current.Length + 1];
        if (current is not null)
        {
            Array.Copy(current, next, current.Length);
            next[^1] = choice;
        }
        SlashCommandBuilder.ValidateChoices(next, _parent.Type);
        _parent.Choices = next;
        return new SlashCommandChoiceBuilder<TValue>(Root, _parent, choice, _parentContainer);
    }

    /// <summary>Callback overload of <see cref="ThenChoice(string, TValue)"/>.</summary>
    public SlashCommandChoiceBuilder<TValue> ThenChoice(string name, TValue value, Action<SlashCommandChoiceBuilder<TValue>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var sibling = ThenChoice(name, value);
        configure(sibling);
        return this;
    }
}

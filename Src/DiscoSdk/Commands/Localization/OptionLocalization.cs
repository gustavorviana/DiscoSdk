namespace DiscoSdk.Commands.Localization;

/// <summary>
/// Translation entry for a single command option (parameter, sub-command, or sub-command group).
/// </summary>
public sealed class OptionLocalization
{
    /// <summary>
    /// The default (non-localized) name of the option. Used to match this localization
    /// against the option on the command.
    /// </summary>
    public required string OptionName { get; init; }

    /// <summary>
    /// Translated display name shown to the user. <c>null</c> or empty leaves the option
    /// without a localized name for the current locale.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Translated description shown to the user. <c>null</c> or empty leaves the option
    /// without a localized description for the current locale.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Nested option translations. Only populated when this option is a sub-command or
    /// sub-command group; matches its children by <see cref="OptionName"/>.
    /// </summary>
    public IReadOnlyList<OptionLocalization>? Options { get; init; }

    /// <summary>
    /// Choice translations for this option. Matches by <see cref="ChoiceLocalization.ChoiceName"/>.
    /// </summary>
    public IReadOnlyList<ChoiceLocalization>? Choices { get; init; }
}

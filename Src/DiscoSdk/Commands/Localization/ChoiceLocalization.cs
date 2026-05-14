namespace DiscoSdk.Commands.Localization;

/// <summary>
/// Translation entry for a single choice on a command option.
/// </summary>
public sealed class ChoiceLocalization
{
    /// <summary>
    /// The default (non-localized) name of the choice. Used to match this localization
    /// against the choice on the command.
    /// </summary>
    public required string ChoiceName { get; init; }

    /// <summary>
    /// Translated display name shown to the user. <c>null</c> or empty leaves the choice
    /// without a localized name for the current locale.
    /// </summary>
    public string? Name { get; init; }
}

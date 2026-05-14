namespace DiscoSdk.Commands.Localization;

/// <summary>
/// Strongly-typed translation tree for a single application command, in a single locale.
/// The shape mirrors the command itself: top-level <see cref="Name"/> / <see cref="Description"/>,
/// then a recursive list of <see cref="OptionLocalization"/>.
/// </summary>
public sealed class CommandLocalization
{
    /// <summary>
    /// Translated command name shown to the user. <c>null</c> or empty leaves the command
    /// without a localized name for the current locale.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Translated command description. <c>null</c> or empty leaves the command without a
    /// localized description for the current locale. Ignored for context-menu commands
    /// (User / Message), which have no description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Translations for the command's options, matched by
    /// <see cref="OptionLocalization.OptionName"/>.
    /// </summary>
    public IReadOnlyList<OptionLocalization>? Options { get; init; }
}

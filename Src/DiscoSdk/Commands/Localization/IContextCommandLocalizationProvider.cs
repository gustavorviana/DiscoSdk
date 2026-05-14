using DiscoSdk.Models;

namespace DiscoSdk.Commands.Localization;

/// <summary>
/// Pluggable contract that supplies translated names for <em>context menu</em> commands
/// (User / Message). Discord exposes only <c>name_localizations</c> on context commands —
/// they have no description and no options — so implementations only need to provide a
/// localized name per locale.
/// </summary>
/// <remarks>
/// <para>
/// The SDK calls <see cref="GetLocalizations"/> once per context menu command. The returned
/// dictionary is keyed by Discord locale code; each value is a <see cref="CommandLocalization"/>
/// where only <see cref="CommandLocalization.Name"/> is populated. The other fields
/// (<see cref="CommandLocalization.Description"/>, <see cref="CommandLocalization.Options"/>)
/// are ignored even if set.
/// </para>
/// <para>
/// Kept as a separate interface from <see cref="ICommandLocalizationProvider"/> so the two
/// systems can be registered independently in DI and consumed by separate localizers in
/// the registration pipeline. Slash and context translations live in different stores and
/// have different validation rules (context names allow mixed case; slash names don't).
/// </para>
/// </remarks>
public interface IContextCommandLocalizationProvider
{
    /// <summary>
    /// Returns translations for <paramref name="commandName"/>, keyed by Discord locale code.
    /// </summary>
    /// <param name="commandName">The default (non-localized) name of the context menu command,
    /// as set via <c>WithName(...)</c> on <c>UserCommandBuilder</c> / <c>MessageCommandBuilder</c>.</param>
    /// <param name="guildId">The guild the command is being registered for, or <c>null</c>
    /// for a global command.</param>
    /// <returns>
    /// A dictionary <c>locale → tree</c>, or <c>null</c> / empty if no translations exist.
    /// Only <see cref="CommandLocalization.Name"/> is consumed by the SDK; other fields are
    /// ignored. Locale keys must be valid Discord locales (see <see cref="DiscordLocales"/>).
    /// </returns>
    IReadOnlyDictionary<string, CommandLocalization>? GetLocalizations(string commandName, Snowflake? guildId);
}

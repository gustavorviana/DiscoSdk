using DiscoSdk.Models;

namespace DiscoSdk.Commands.Localization;

/// <summary>
/// Pluggable contract that supplies translated names and descriptions for application
/// commands. Implementations decide where translations come from (in-memory dictionaries,
/// JSON / YAML files, .resx resources, Microsoft <c>IStringLocalizer</c>, a database,
/// a remote service, etc.).
/// </summary>
/// <remarks>
/// <para>
/// The SDK calls <see cref="GetLocalizations"/> once per command. The returned dictionary
/// is keyed by Discord locale code (e.g. <c>"pt-BR"</c>, <c>"en-US"</c>); each value is a
/// strongly-typed <see cref="CommandLocalization"/> tree describing the translations for
/// that locale.
/// </para>
/// <para>
/// Manual localizations set via <c>SlashCommandBuilder.AddNameLocalization(...)</c> or
/// equivalent builder calls always take precedence — the provider only fills locales that
/// are not already populated.
/// </para>
/// <para>
/// The optional <paramref name="guildId"/> argument is <c>null</c> when the command is
/// being registered as a global command, or the snowflake of the guild when it is being
/// registered as a guild-specific command. Implementations that want different
/// translations per guild (multi-tenant bots, per-server overrides) can branch on this
/// value; implementations that don't care can simply ignore it.
/// </para>
/// </remarks>
public interface ICommandLocalizationProvider
{
    /// <summary>
    /// Returns translations for <paramref name="commandName"/>, keyed by Discord locale code.
    /// </summary>
    /// <param name="commandName">The default (non-localized) name of the command, as set
    /// via <c>WithName(...)</c> on the builder.</param>
    /// <param name="guildId">The guild the command is being registered for, or <c>null</c>
    /// for a global command.</param>
    /// <returns>
    /// A dictionary <c>locale → tree</c>, or <c>null</c> / empty if no translations exist
    /// for the command. Locale keys must be valid Discord locales (see <see cref="DiscordLocales"/>);
    /// invalid locales cause the SDK to throw <see cref="InvalidOperationException"/>.
    /// </returns>
    IReadOnlyDictionary<string, CommandLocalization>? GetLocalizations(string commandName, Snowflake? guildId);
}

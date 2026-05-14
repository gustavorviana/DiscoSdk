using DiscoSdk.Models;

namespace DiscoSdk.Commands.Localization;

/// <summary>
/// In-memory implementation of <see cref="IContextCommandLocalizationProvider"/>. Storage is
/// fully independent from <see cref="InMemoryCommandLocalizationProvider"/> (slash) — context
/// translations are registered against a separate global and per-guild table.
/// </summary>
/// <remarks>
/// <para>Lookup semantics mirror the slash provider:</para>
/// <list type="bullet">
/// <item><description>When <c>guildId == null</c>, returns the entry from the global table
/// (or <c>null</c> if not registered).</description></item>
/// <item><description>When <c>guildId</c> is given and a per-guild entry exists for the
/// command, returns it.</description></item>
/// <item><description>When the guild has no entry: in <c>strict</c> mode returns <c>null</c>;
/// otherwise falls back to the global table.</description></item>
/// </list>
/// </remarks>
public sealed class InMemoryContextCommandLocalizationProvider : IContextCommandLocalizationProvider
{
    private readonly bool _strict;

    private readonly Dictionary<string, IReadOnlyDictionary<string, CommandLocalization>> _global = new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<Snowflake, Dictionary<string, IReadOnlyDictionary<string, CommandLocalization>>> _perGuild = new();

    /// <summary>
    /// Creates a non-strict provider. Lookups for a guild that has no specific translation
    /// fall back to the global table.
    /// </summary>
    public InMemoryContextCommandLocalizationProvider() : this(strict: false) { }

    /// <summary>
    /// Creates a provider with explicit strictness.
    /// </summary>
    /// <param name="strict">
    /// When <c>true</c>, a lookup with a <c>guildId</c> that has no specific entry for the
    /// command returns <c>null</c> — no fallback to the global table. When <c>false</c>,
    /// the global table is used as fallback.
    /// </param>
    public InMemoryContextCommandLocalizationProvider(bool strict)
    {
        _strict = strict;
    }

    /// <summary>
    /// Gets whether this provider falls back to the global table for guild lookups
    /// without specific translations.
    /// </summary>
    public bool Strict => _strict;

    /// <summary>
    /// Registers (or replaces) the translations for a context menu command in the global table.
    /// </summary>
    public InMemoryContextCommandLocalizationProvider RegisterCommand(
        string commandName,
        IReadOnlyDictionary<string, CommandLocalization> translations)
    {
        EnsureCommandName(commandName);
        ArgumentNullException.ThrowIfNull(translations);

        _global[commandName] = translations;
        return this;
    }

    /// <summary>
    /// Convenience overload that accepts a flat <c>locale → name</c> dictionary and wraps each
    /// entry into a <see cref="CommandLocalization"/> with only <see cref="CommandLocalization.Name"/>
    /// populated — the only field that matters for context commands.
    /// </summary>
    public InMemoryContextCommandLocalizationProvider RegisterCommand(
        string commandName,
        IReadOnlyDictionary<string, string> namesByLocale)
    {
        EnsureCommandName(commandName);
        ArgumentNullException.ThrowIfNull(namesByLocale);

        _global[commandName] = WrapNames(namesByLocale);
        return this;
    }

    /// <summary>
    /// Registers (or replaces) the translations for a context menu command, scoped to a
    /// specific guild. These take precedence over the global table for that guild.
    /// </summary>
    public InMemoryContextCommandLocalizationProvider RegisterCommand(
        Snowflake guildId,
        string commandName,
        IReadOnlyDictionary<string, CommandLocalization> translations)
    {
        EnsureCommandName(commandName);
        ArgumentNullException.ThrowIfNull(translations);

        EnsureGuildBucket(guildId)[commandName] = translations;
        return this;
    }

    /// <summary>
    /// Per-guild overload of <see cref="RegisterCommand(string, IReadOnlyDictionary{string, string})"/>.
    /// </summary>
    public InMemoryContextCommandLocalizationProvider RegisterCommand(
        Snowflake guildId,
        string commandName,
        IReadOnlyDictionary<string, string> namesByLocale)
    {
        EnsureCommandName(commandName);
        ArgumentNullException.ThrowIfNull(namesByLocale);

        EnsureGuildBucket(guildId)[commandName] = WrapNames(namesByLocale);
        return this;
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, CommandLocalization>? GetLocalizations(string commandName, Snowflake? guildId)
    {
        if (string.IsNullOrEmpty(commandName))
            return null;

        if (guildId is null)
            return _global.GetValueOrDefault(commandName);

        if (_perGuild.TryGetValue(guildId.Value, out var byCmd)
            && byCmd.TryGetValue(commandName, out var perGuild))
            return perGuild;

        if (_strict)
            return null;

        return _global.GetValueOrDefault(commandName);
    }

    private Dictionary<string, IReadOnlyDictionary<string, CommandLocalization>> EnsureGuildBucket(Snowflake guildId)
    {
        if (!_perGuild.TryGetValue(guildId, out var byCmd))
        {
            byCmd = new Dictionary<string, IReadOnlyDictionary<string, CommandLocalization>>(StringComparer.OrdinalIgnoreCase);
            _perGuild[guildId] = byCmd;
        }
        return byCmd;
    }

    private static IReadOnlyDictionary<string, CommandLocalization> WrapNames(IReadOnlyDictionary<string, string> namesByLocale)
    {
        var wrapped = new Dictionary<string, CommandLocalization>(StringComparer.OrdinalIgnoreCase);
        foreach (var (locale, name) in namesByLocale)
            wrapped[locale] = new CommandLocalization { Name = name };
        return wrapped;
    }

    private static void EnsureCommandName(string commandName)
    {
        if (string.IsNullOrWhiteSpace(commandName))
            throw new ArgumentException("Command name cannot be null or empty.", nameof(commandName));
    }
}

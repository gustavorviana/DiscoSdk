using DiscoSdk.Models;

namespace DiscoSdk.Commands.Localization;

/// <summary>
/// In-memory implementation of <see cref="ICommandLocalizationProvider"/> backed by
/// two tables: a global one keyed by command name, and a per-guild one keyed by
/// <c>(guildId, commandName)</c>.
/// </summary>
/// <remarks>
/// <para>Lookup semantics:</para>
/// <list type="bullet">
/// <item>
/// <description>When <c>guildId == null</c>, returns the entry from the global table
/// (or <c>null</c> if not registered).</description>
/// </item>
/// <item>
/// <description>When <c>guildId</c> is given and a per-guild entry exists for the
/// command, returns it.</description>
/// </item>
/// <item>
/// <description>When the guild has no entry for the command: in <c>strict</c> mode
/// returns <c>null</c>; otherwise falls back to the global table.</description>
/// </item>
/// </list>
/// </remarks>
public sealed class InMemoryCommandLocalizationProvider : ICommandLocalizationProvider
{
    private readonly bool _strict;

    private readonly Dictionary<string, IReadOnlyDictionary<string, CommandLocalization>> _global = new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<Snowflake, Dictionary<string, IReadOnlyDictionary<string, CommandLocalization>>> _perGuild = new();

    /// <summary>
    /// Creates a non-strict provider. Lookups for a guild that has no specific
    /// translation fall back to the global table.
    /// </summary>
    public InMemoryCommandLocalizationProvider() : this(strict: false) { }

    /// <summary>
    /// Creates a provider with explicit strictness.
    /// </summary>
    /// <param name="strict">
    /// When <c>true</c>, a lookup with a <c>guildId</c> that has no specific entry for the
    /// command returns <c>null</c> — no fallback to the global table. When <c>false</c>,
    /// the global table is used as fallback.
    /// </param>
    public InMemoryCommandLocalizationProvider(bool strict)
    {
        _strict = strict;
    }

    /// <summary>
    /// Gets whether this provider falls back to the global table for guild lookups
    /// without specific translations.
    /// </summary>
    public bool Strict => _strict;

    /// <summary>
    /// Registers (or replaces) the translations for a command in the global table.
    /// </summary>
    public InMemoryCommandLocalizationProvider RegisterCommand(
        string commandName,
        IReadOnlyDictionary<string, CommandLocalization> translations)
    {
        EnsureCommandName(commandName);
        ArgumentNullException.ThrowIfNull(translations);

        _global[commandName] = translations;
        return this;
    }

    /// <summary>
    /// Registers (or replaces) the translations for a command, scoped to a specific guild.
    /// These translations take precedence over the global ones for that guild.
    /// </summary>
    public InMemoryCommandLocalizationProvider RegisterCommand(
        Snowflake guildId,
        string commandName,
        IReadOnlyDictionary<string, CommandLocalization> translations)
    {
        EnsureCommandName(commandName);
        ArgumentNullException.ThrowIfNull(translations);

        if (!_perGuild.TryGetValue(guildId, out var byCmd))
        {
            byCmd = new Dictionary<string, IReadOnlyDictionary<string, CommandLocalization>>(StringComparer.OrdinalIgnoreCase);
            _perGuild[guildId] = byCmd;
        }

        byCmd[commandName] = translations;
        return this;
    }

    /// <summary>
    /// Fluent overload of <see cref="RegisterCommand(string, IReadOnlyDictionary{string, CommandLocalization})"/>
    /// — drives a <see cref="CommandLocalizationBuilder"/> via the supplied callback, then registers
    /// the built dictionary in the global table.
    /// </summary>
    /// <example>
    /// <code>
    /// provider.DefineCommand("ban", b => b
    ///     .ForLocale("en-GB", t => t
    ///         .WithName("ban")
    ///         .WithDescription("Ban a user")
    ///         .Option("user").WithName("target")
    ///         .Option("reason").WithName("cause")
    ///             .ThenChoice("spam", "Spam")
    ///             .ThenChoice("flood", "Flooding")));
    /// </code>
    /// </example>
    public InMemoryCommandLocalizationProvider DefineCommand(
        string commandName,
        Action<CommandLocalizationBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var builder = new CommandLocalizationBuilder();
        configure(builder);
        return RegisterCommand(commandName, builder.Build());
    }

    /// <summary>
    /// Per-guild fluent overload of <see cref="RegisterCommand(Snowflake, string, IReadOnlyDictionary{string, CommandLocalization})"/>.
    /// Mirrors <see cref="DefineCommand(string, Action{CommandLocalizationBuilder})"/> but registers
    /// the result under <paramref name="guildId"/>, taking precedence over the global table for that guild.
    /// </summary>
    public InMemoryCommandLocalizationProvider DefineCommandForGuild(
        Snowflake guildId,
        string commandName,
        Action<CommandLocalizationBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var builder = new CommandLocalizationBuilder();
        configure(builder);
        return RegisterCommand(guildId, commandName, builder.Build());
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

    private static void EnsureCommandName(string commandName)
    {
        if (string.IsNullOrWhiteSpace(commandName))
            throw new ArgumentException("Command name cannot be null or empty.", nameof(commandName));
    }
}

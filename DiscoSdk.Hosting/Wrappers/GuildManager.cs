using DiscoSdk.Hosting.Logging;
using DiscoSdk.Logging;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using System.Collections.Concurrent;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Manages guild cache, pending guilds tracking, and guild-related operations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GuildManager"/> class.
/// </remarks>
/// <param name="client">The REST client base to use for API requests.</param>
/// <param name="logger">The logger instance. If null, uses NullLogger.</param>
public class GuildManager(DiscordClient client, ILogger? logger = null)
{
    private readonly ConcurrentDictionary<Snowflake, IGuild> _guildCache = [];
    private readonly HashSet<Snowflake> _pendingGuilds = [];
    private readonly object _lock = new();
    private readonly ILogger _logger = logger ?? NullLogger.Instance;

    /// <summary>
    /// Gets a value indicating whether all guilds have been loaded.
    /// </summary>
    public bool IsFullyInitialized
    {
        get
        {
            lock (_lock)
                return _pendingGuilds.Count == 0;
        }
    }

    /// <summary>
    /// Gets the number of guilds that are still pending to be loaded.
    /// </summary>
    public int PendingGuildsCount
    {
        get
        {
            lock (_lock)
                return _pendingGuilds.Count;
        }
    }

    /// <summary>
    /// Gets a read-only dictionary of cached guilds.
    /// </summary>
    public IReadOnlyDictionary<Snowflake, IGuild> All => _guildCache;

    /// <summary>
    /// Initializes the pending guilds list from the Ready payload.
    /// </summary>
    /// <param name="guildIds">The list of guild IDs from the Ready payload.</param>
    internal void InitializePendingGuilds(IEnumerable<Snowflake> guildIds)
    {
        lock (_lock)
        {
            _pendingGuilds.Clear();
            foreach (var guildId in guildIds)
                if (!guildId.Empty)
                    _pendingGuilds.Add(guildId);

            _logger.Log(LogLevel.Information, $"Initialized {_pendingGuilds.Count} pending guild(s) from Ready payload.");
        }
    }

    /// <summary>
    /// Handles a GUILD_CREATE event to update the guild cache and pending list.
    /// </summary>
    /// <param name="guild">The guild that was created or became available.</param>
    /// <param name="jsonOptions">The JSON serializer options.</param>
    internal IGuild? HandleGuildCreate(Guild? guild)
    {
        if (guild == null || !guild.Id.Empty)
            return null;

        lock (_lock)
        {
            var wrappedGuild = new GuildWrapper(guild, client);

            _guildCache[guild.Id] = wrappedGuild;

            if (_pendingGuilds.Remove(guild.Id))
            {
                var remaining = _pendingGuilds.Count;
                _logger.Log(LogLevel.Debug, $"Guild {guild.Name} ({guild.Id}) loaded. {remaining} guild(s) remaining.");

                if (remaining == 0)
                    _logger.Log(LogLevel.Information, $"All guilds loaded! Bot is fully initialized. Total guilds: {_guildCache.Count}");
            }

            return wrappedGuild;
        }
    }

    internal IGuild? HandleGuildUpdate(Guild? guildUpdate)
    {
        if (guildUpdate is null || _guildCache.TryGetValue(guildUpdate.Id, out var guild))
            return null;

        (guild as GuildWrapper)?.OnUpdate(guildUpdate);
        return guild;
    }

    internal void HandleGuildDelete(Snowflake guildId)
    {
        _guildCache.Remove(guildId, out _);
    }

    internal void HandleChannelCreate(Channel channel)
    {
        if (channel.GuildId == null || !_guildCache.TryGetValue(channel.GuildId.Value, out var guild))
            return;

        (guild as GuildWrapper)?.OnChannelAdd(channel);
    }

    internal void HandleChannelUpdate(Channel channel)
    {
        if (channel.GuildId == null || !_guildCache.TryGetValue(channel.GuildId.Value, out var guild))
            return;

        (guild as GuildWrapper)?.OnChannelUpdate(channel);
    }

    internal void HandleChannelDelete(Channel channel)
    {
        if (channel.GuildId == null || !_guildCache.TryGetValue(channel.GuildId.Value, out var guild))
            return;

        (guild as GuildWrapper)?.OnChannelDelete(channel.Id);
    }

    /// <summary>
    /// Gets a guild by its ID. First checks the cache, and if not found, fetches from Discord API.
    /// </summary>
    /// <param name="guildId">The guild ID.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>The guild if found; otherwise, <c>null</c>.</returns>
    public async Task<IGuild?> GetAsync(Snowflake guildId, CancellationToken ct = default)
    {
        if (guildId.Empty)
            return null;

        if (_guildCache.TryGetValue(guildId, out var cachedGuild))
            return cachedGuild;

        try
        {
            var guild = await client.GuildClient.GetAsync(guildId, ct);

            if (guild != null && !guildId.Empty)
            {
                var wrapped = new GuildWrapper(guild, client);
                _guildCache[guild.Id] = wrapped;
                return wrapped;
            }
        }
        catch
        {
        }

        return null;
    }

    public bool TryGet(Snowflake guildId, out IGuild? guild)
    {
        if (guildId.Empty)
        {
            guild = null;
            return false;
        }

        return _guildCache.TryGetValue(guildId, out guild);
    }
}
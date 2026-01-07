using DiscoSdk.Hosting.Logging;
using DiscoSdk.Hosting.Rest;
using DiscoSdk.Logging;
using DiscoSdk.Models;
using System.Collections.Concurrent;

namespace DiscoSdk.Hosting.Guilds;

/// <summary>
/// Manages guild cache, pending guilds tracking, and guild-related operations.
/// </summary>
public class GuildManager
{
    private readonly ConcurrentDictionary<string, Guild> _guildCache = [];
    private readonly HashSet<string> _pendingGuilds = [];
    private readonly IDiscordRestClientBase _client;
    private readonly ILogger _logger;
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GuildManager"/> class.
    /// </summary>
    /// <param name="client">The REST client base to use for API requests.</param>
    /// <param name="logger">The logger instance. If null, uses NullLogger.</param>
    public GuildManager(IDiscordRestClientBase client, ILogger? logger = null)
    {
        _client = client;
        _logger = logger ?? NullLogger.Instance;
    }

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
    public IReadOnlyDictionary<string, Guild> All => _guildCache;

    /// <summary>
    /// Initializes the pending guilds list from the Ready payload.
    /// </summary>
    /// <param name="guildIds">The list of guild IDs from the Ready payload.</param>
    internal void InitializePendingGuilds(IEnumerable<string> guildIds)
    {
        lock (_lock)
        {
            _pendingGuilds.Clear();
            foreach (var guildId in guildIds)
            {
                if (!string.IsNullOrEmpty(guildId))
                {
                    _pendingGuilds.Add(guildId);
                }
            }
            _logger.Log(LogLevel.Information, $"Initialized {_pendingGuilds.Count} pending guild(s) from Ready payload.");
        }
    }

    /// <summary>
    /// Handles a GUILD_CREATE event to update the guild cache and pending list.
    /// </summary>
    /// <param name="guild">The guild that was created or became available.</param>
    /// <param name="jsonOptions">The JSON serializer options.</param>
    internal void HandleGuildCreate(Guild guild)
    {
        if (guild == null || string.IsNullOrEmpty(guild.Id))
            return;

        lock (_lock)
        {
            // Add or update guild in cache
            _guildCache[guild.Id] = guild;

            // Remove from pending list if it was there
            if (_pendingGuilds.Remove(guild.Id))
            {
                var remaining = _pendingGuilds.Count;
                _logger.Log(LogLevel.Debug, $"Guild {guild.Name} ({guild.Id}) loaded. {remaining} guild(s) remaining.");

                // If all guilds are loaded, log completion
                if (remaining == 0)
                {
                    _logger.Log(LogLevel.Information, $"All guilds loaded! Bot is fully initialized. Total guilds: {_guildCache.Count}");
                }
            }
        }
    }

    /// <summary>
    /// Gets a guild by its ID. First checks the cache, and if not found, fetches from Discord API.
    /// </summary>
    /// <param name="guildId">The guild ID.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>The guild if found; otherwise, <c>null</c>.</returns>
    public async Task<Guild?> GetAsync(string guildId, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(guildId))
            return null;

        // First, try to get from cache
        if (_guildCache.TryGetValue(guildId, out var cachedGuild))
            return cachedGuild;

        // If not in cache and we have a client, fetch from API
        try
        {
            var guild = await _client.SendJsonAsync<Guild>($"guilds/{guildId}", HttpMethod.Get, null, ct);

            if (guild != null && !string.IsNullOrEmpty(guild.Id))
            {
                // Add to cache
                _guildCache[guild.Id] = guild;
                return guild;
            }
        }
        catch
        {
            // Guild not found or error - return null
            return null;
        }

        return null;
    }
}


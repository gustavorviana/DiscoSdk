using DiscoSdk.Caching;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace DiscoSdk.Hosting.Managers;

/// <summary>
/// In-memory cache for guild-uploaded stickers plus the cache/REST routing logic exposed by
/// <see cref="IStickerManager"/>. The cache is fed by the <c>GUILD_CREATE</c> snapshot and
/// refreshed wholesale on every <c>GUILD_STICKERS_UPDATE</c> event; the update event replaces the
/// partition entirely so deleted stickers fall out automatically.
/// </summary>
internal sealed class StickerManager : IStickerManager
{
    /// <summary>Default flag set when the host does not register a custom value via DI.</summary>
    internal const StickerCacheFlag DefaultFlags = StickerCacheFlag.None;

    private readonly DiscordClient _client;
    private readonly StickerCacheFlag _flags;
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<Snowflake, ConcurrentDictionary<Snowflake, Sticker>> _byGuild = new();

    internal StickerManager(
        DiscordClient client,
        StickerCacheFlag flags = DefaultFlags,
        ILogger? logger = null)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _flags = flags;
        _logger = logger ?? NullLogger.Instance;
    }

    internal StickerCacheFlag Flags => _flags;

    /// <inheritdoc />
    public async ValueTask<ISticker?> GetAsync(
        Snowflake guildId,
        Snowflake stickerId,
        StickerFetchMode mode = StickerFetchMode.CacheThenRest,
        CancellationToken ct = default)
    {
        if (guildId.Empty || stickerId.Empty)
            return null;

        if (mode != StickerFetchMode.RestOnly
            && _byGuild.TryGetValue(guildId, out var guildCache)
            && guildCache.TryGetValue(stickerId, out var cached))
        {
            return new StickerWrapper(_client, cached);
        }

        if (mode == StickerFetchMode.CacheOnly)
            return null;

        Sticker? fresh;
        try
        {
            fresh = await _client.StickerClient.GetGuildStickerAsync(guildId, stickerId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch sticker {StickerId} in guild {GuildId} from REST.", stickerId, guildId);
            return null;
        }

        if (fresh is null)
            return null;

        if (_flags == StickerCacheFlag.Guild)
            GetOrAddGuildCache(guildId)[stickerId] = fresh;

        return new StickerWrapper(_client, fresh);
    }

    /// <inheritdoc />
    public IGuildStickerScope OfGuild(Snowflake guildId) => new GuildStickerScope(this, guildId);

    /// <summary>
    /// Seeds the cache from the <c>GUILD_CREATE</c> sticker snapshot. The supplied list is
    /// authoritative — any prior partition for this guild is dropped first.
    /// </summary>
    internal void OnGuildStickersSeed(IEnumerable<Sticker>? stickers, Snowflake guildId)
    {
        if (_flags != StickerCacheFlag.Guild || guildId.Empty)
            return;

        ReplacePartition(guildId, stickers);
    }

    /// <summary>
    /// Handles <c>GUILD_STICKERS_UPDATE</c>. Discord ships the full list on every change, so the
    /// SDK replaces the partition wholesale to keep deleted stickers from lingering.
    /// </summary>
    internal void OnGuildStickersUpdate(IEnumerable<Sticker>? stickers, Snowflake guildId)
    {
        if (_flags != StickerCacheFlag.Guild || guildId.Empty)
            return;

        ReplacePartition(guildId, stickers);
    }

    /// <summary>Drops the per-guild partition when the bot leaves the guild.</summary>
    internal void OnGuildRemove(Snowflake guildId)
    {
        if (!guildId.Empty)
            _byGuild.TryRemove(guildId, out _);
    }

    internal IReadOnlyList<ISticker> GetCached(Snowflake guildId)
    {
        if (guildId.Empty || !_byGuild.TryGetValue(guildId, out var guildCache) || guildCache.IsEmpty)
            return [];

        var builder = ImmutableArray.CreateBuilder<ISticker>(guildCache.Count);
        foreach (var pair in guildCache)
            builder.Add(new StickerWrapper(_client, pair.Value));

        return builder.ToImmutable();
    }

    internal async ValueTask<IReadOnlyList<ISticker>> GetAllAsync(
        Snowflake guildId,
        StickerFetchMode mode,
        CancellationToken ct)
    {
        if (guildId.Empty)
            return [];

        if (mode != StickerFetchMode.RestOnly)
        {
            var snapshot = GetCached(guildId);
            if (snapshot.Count > 0)
                return snapshot;
        }

        if (mode == StickerFetchMode.CacheOnly)
            return [];

        Sticker[] fresh;
        try
        {
            fresh = await _client.StickerClient.ListGuildStickersAsync(guildId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to list stickers for guild {GuildId} from REST.", guildId);
            return [];
        }

        if (_flags == StickerCacheFlag.Guild)
            ReplacePartition(guildId, fresh);

        var builder = ImmutableArray.CreateBuilder<ISticker>(fresh.Length);
        foreach (var sticker in fresh)
            builder.Add(new StickerWrapper(_client, sticker));
        return builder.ToImmutable();
    }

    private ConcurrentDictionary<Snowflake, Sticker> GetOrAddGuildCache(Snowflake guildId)
        => _byGuild.GetOrAdd(guildId, static _ => new ConcurrentDictionary<Snowflake, Sticker>());

    private void ReplacePartition(Snowflake guildId, IEnumerable<Sticker>? stickers)
    {
        var fresh = new ConcurrentDictionary<Snowflake, Sticker>();
        if (stickers is not null)
        {
            foreach (var sticker in stickers)
            {
                if (sticker is null || sticker.Id.Empty)
                    continue;
                fresh[sticker.Id] = sticker;
            }
        }

        _byGuild[guildId] = fresh;
    }

    private sealed class GuildStickerScope(StickerManager manager, Snowflake guildId) : IGuildStickerScope
    {
        public Snowflake GuildId => guildId;

        public ValueTask<ISticker?> GetAsync(
            Snowflake stickerId,
            StickerFetchMode mode = StickerFetchMode.CacheThenRest,
            CancellationToken ct = default)
            => manager.GetAsync(guildId, stickerId, mode, ct);

        public ValueTask<IReadOnlyList<ISticker>> GetAllAsync(
            StickerFetchMode mode = StickerFetchMode.CacheThenRest,
            CancellationToken ct = default)
            => manager.GetAllAsync(guildId, mode, ct);

        public IReadOnlyList<ISticker> GetCached() => manager.GetCached(guildId);
    }
}

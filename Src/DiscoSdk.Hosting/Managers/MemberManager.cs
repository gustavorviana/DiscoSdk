using DiscoSdk.Caching;
using DiscoSdk.Hosting.Caching.Policies;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace DiscoSdk.Hosting.Managers;

/// <summary>
/// Default <see cref="IMemberManager"/> implementation. Members live in a nested
/// <see cref="ConcurrentDictionary{TKey, TValue}"/> partitioned by guild — the outer map keys by
/// guild id and the inner map keys by user id. Both levels are concurrent so the gateway
/// dispatcher can process multiple members of the same guild in parallel without external locking.
/// </summary>
internal sealed class MemberManager : IMemberManager
{
    private readonly DiscordClient _client;
    private readonly IMemberCachePolicy _policy;
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<Snowflake, ConcurrentDictionary<Snowflake, GuildMember>> _byGuild = new();

    public MemberManager(
        DiscordClient client,
        IMemberCachePolicy? policy = null,
        ILogger? logger = null)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _policy = policy ?? AllPolicy.Instance;
        _logger = logger ?? NullLogger.Instance;
    }

    /// <inheritdoc />
    public async ValueTask<IMember?> GetAsync(
        Snowflake guildId,
        Snowflake userId,
        MemberFetchMode mode = MemberFetchMode.CacheThenRest,
        CancellationToken ct = default)
    {
        if (guildId.Empty || userId.Empty)
            return null;

        if (mode != MemberFetchMode.RestOnly
            && _byGuild.TryGetValue(guildId, out var guildCache)
            && guildCache.TryGetValue(userId, out var cached))
        {
            return Wrap(cached, guildId);
        }

        if (mode == MemberFetchMode.CacheOnly)
            return null;

        GuildMember? fresh;
        try
        {
            fresh = await _client.GuildClient.GetMemberAsync(guildId, userId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch member {UserId} in guild {GuildId} from REST.", userId, guildId);
            return null;
        }

        if (fresh is null)
            return null;

        var wrapped = Wrap(fresh, guildId);
        if (wrapped is not null && _policy.ShouldCache(wrapped))
            GetOrAddGuildCache(guildId)[userId] = fresh;

        return wrapped;
    }

    /// <inheritdoc />
    public IGuildMemberScope OfGuild(Snowflake guildId) => new GuildMemberScope(this, guildId);

    /// <summary>
    /// Called by the gateway dispatcher on <c>GUILD_MEMBER_ADD</c> and <c>GUILD_MEMBER_UPDATE</c>.
    /// Upserts the entry when the policy accepts the member; evicts a previously cached entry
    /// when the policy now rejects it.
    /// </summary>
    internal ValueTask OnMemberAddOrUpdateAsync(
        GuildMember member,
        Snowflake guildId,
        CancellationToken ct = default)
    {
        var userId = member.User?.UserId ?? default;
        if (userId.Empty || guildId.Empty)
            return ValueTask.CompletedTask;

        var wrapped = Wrap(member, guildId);
        if (wrapped is null)
            return ValueTask.CompletedTask;

        if (_policy.ShouldCache(wrapped))
        {
            GetOrAddGuildCache(guildId)[userId] = member;
        }
        else if (_byGuild.TryGetValue(guildId, out var guildCache))
        {
            guildCache.TryRemove(userId, out _);
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Called by the gateway dispatcher on <c>GUILD_MEMBER_REMOVE</c>.
    /// </summary>
    internal ValueTask OnMemberRemoveAsync(Snowflake guildId, Snowflake userId, CancellationToken ct = default)
    {
        if (guildId.Empty || userId.Empty)
            return ValueTask.CompletedTask;

        if (_byGuild.TryGetValue(guildId, out var guildCache))
            guildCache.TryRemove(userId, out _);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Seeds the cache from the <c>GUILD_CREATE</c> member snapshot. Every entry is evaluated
    /// against the configured policy individually; entries the policy rejects are skipped. Runs
    /// synchronously so <see cref="GuildManager.HandleGuildCreate"/> can populate the cache
    /// before its lock releases.
    /// </summary>
    internal void OnGuildMembersSeed(IReadOnlyList<GuildMember>? members, Snowflake guildId)
    {
        if (members is null || members.Count == 0 || guildId.Empty)
            return;

        foreach (var member in members)
        {
            if (member is null)
                continue;

            var userId = member.User?.UserId ?? default;
            if (userId.Empty)
                continue;

            var wrapped = Wrap(member, guildId);
            if (wrapped is null)
                continue;

            if (_policy.ShouldCache(wrapped))
                GetOrAddGuildCache(guildId)[userId] = member;
        }
    }

    /// <summary>
    /// Called by the gateway dispatcher on <c>GUILD_MEMBERS_CHUNK</c>. Each entry is processed
    /// through the policy individually.
    /// </summary>
    internal async ValueTask OnMembersChunkAsync(
        IReadOnlyList<GuildMember> chunk,
        Snowflake guildId,
        CancellationToken ct = default)
    {
        if (chunk is null || chunk.Count == 0 || guildId.Empty)
            return;

        foreach (var member in chunk)
            await OnMemberAddOrUpdateAsync(member, guildId, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Drops every cache entry for the supplied guild — the entire partitioned dictionary is
    /// removed so GC can reclaim the inner map.
    /// </summary>
    internal ValueTask OnGuildRemoveAsync(Snowflake guildId, CancellationToken ct = default)
    {
        if (!guildId.Empty)
            _byGuild.TryRemove(guildId, out _);
        return ValueTask.CompletedTask;
    }

    internal async IAsyncEnumerable<IMember> EnumerateGuildAsync(
        Snowflake guildId,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (!_byGuild.TryGetValue(guildId, out var guildCache))
            yield break;

        foreach (var pair in guildCache)
        {
            ct.ThrowIfCancellationRequested();
            var wrapped = Wrap(pair.Value, guildId);
            if (wrapped is not null)
                yield return wrapped;
            await Task.Yield();
        }
    }

    internal ValueTask<int> CountGuildAsync(Snowflake guildId, CancellationToken ct = default)
        => _byGuild.TryGetValue(guildId, out var guildCache)
            ? new ValueTask<int>(guildCache.Count)
            : new ValueTask<int>(0);

    private ConcurrentDictionary<Snowflake, GuildMember> GetOrAddGuildCache(Snowflake guildId)
        => _byGuild.GetOrAdd(guildId, static _ => new ConcurrentDictionary<Snowflake, GuildMember>());

    private IMember? Wrap(GuildMember poco, Snowflake guildId)
    {
        var guild = _client.Guilds.GetWrapped(guildId);
        if (guild is null)
            return null;

        return new GuildMemberWrapper(_client, poco, guild);
    }

    private sealed class GuildMemberScope(MemberManager manager, Snowflake guildId) : IGuildMemberScope
    {
        public Snowflake GuildId => guildId;

        public ValueTask<IMember?> GetAsync(
            Snowflake userId,
            MemberFetchMode mode = MemberFetchMode.CacheThenRest,
            CancellationToken ct = default)
            => manager.GetAsync(guildId, userId, mode, ct);

        public IAsyncEnumerable<IMember> GetCachedAsync(CancellationToken ct = default)
            => manager.EnumerateGuildAsync(guildId, ct);

        public ValueTask<int> GetCachedCountAsync(CancellationToken ct = default)
            => manager.CountGuildAsync(guildId, ct);
    }
}

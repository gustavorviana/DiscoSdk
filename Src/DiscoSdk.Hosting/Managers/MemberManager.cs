using DiscoSdk.Caching;
using DiscoSdk.Hosting.Caching.Policies;
using DiscoSdk.Hosting.Observability;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;

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
            RecordCacheLookup("hit");
            return Wrap(cached, guildId);
        }

        if (mode == MemberFetchMode.CacheOnly)
        {
            RecordCacheLookup("miss");
            return null;
        }

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
        {
            RecordCacheLookup("miss");
            return null;
        }

        var wrapped = Wrap(fresh, guildId);
        if (wrapped is not null && _policy.ShouldCache(wrapped))
            GetOrAddGuildCache(guildId)[userId] = fresh;

        RecordCacheLookup("rest");
        return wrapped;
    }

    private static void RecordCacheLookup(string result)
        => DiscoSdkDiagnostics.CacheLookups.Add(
            1,
            new KeyValuePair<string, object?>(DiagnosticTags.CacheEntity, DiagnosticTags.CacheEntityMember),
            new KeyValuePair<string, object?>(DiagnosticTags.CacheResult, result));

    /// <inheritdoc />
    public IGuildMembers OfGuild(Snowflake guildId) => new GuildMembersImpl(this, guildId, null);

    /// <summary>
    /// Hosting-internal overload that pre-binds an <see cref="IGuild"/> context, avoiding a cache
    /// lookup when the caller already has the wrapper in hand. Used by <c>GuildWrapper</c> so
    /// member REST builders work even before the guild is materialized in <c>GuildManager</c>.
    /// </summary>
    internal IGuildMembers OfGuild(Snowflake guildId, IGuild guildContext)
        => new GuildMembersImpl(this, guildId, guildContext);

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

    internal IEnumerable<IMember> EnumerateGuild(Snowflake guildId)
    {
        if (!_byGuild.TryGetValue(guildId, out var guildCache))
            yield break;

        foreach (var pair in guildCache)
        {
            var wrapped = Wrap(pair.Value, guildId);
            if (wrapped is not null)
                yield return wrapped;
        }
    }

    internal int CountGuild(Snowflake guildId)
        => _byGuild.TryGetValue(guildId, out var guildCache) ? guildCache.Count : 0;

    private ConcurrentDictionary<Snowflake, GuildMember> GetOrAddGuildCache(Snowflake guildId)
        => _byGuild.GetOrAdd(guildId, static _ => new ConcurrentDictionary<Snowflake, GuildMember>());

    private IMember? Wrap(GuildMember poco, Snowflake guildId)
    {
        var guild = _client.Guilds.GetWrapped(guildId);
        if (guild is null)
            return null;

        return new GuildMemberWrapper(_client, poco, guild);
    }

    private sealed class GuildMembersImpl(MemberManager manager, Snowflake guildId, IGuild? guildContext) : IGuildMembers
    {
        public Snowflake GuildId => guildId;

        public IRestAction<IMember?> Get(Snowflake userId, MemberFetchMode mode = MemberFetchMode.CacheThenRest)
        {
            var gid = guildId;
            return RestAction<IMember?>.Create(async ct =>
                await manager.GetAsync(gid, userId, mode, ct).ConfigureAwait(false));
        }

        public IEnumerable<IMember> GetCached() => manager.EnumerateGuild(guildId);

        public int GetCachedCount() => manager.CountGuild(guildId);

        public IMemberPaginationAction List()
            => new MemberPaginationAction(manager._client, ResolveGuild());

        public IRestAction<IReadOnlyList<IMember>> Search(string query, int? limit = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query cannot be null or empty.", nameof(query));

            var client = manager._client;
            var gid = guildId;
            return RestAction<IReadOnlyList<IMember>>.Create(async ct =>
            {
                var members = await client.GuildClient.SearchMembersAsync(gid, query, limit, ct).ConfigureAwait(false);
                var guild = ResolveGuild();
                return members
                    .Where(m => m.User != null)
                    .Select(m => (IMember)new GuildMemberWrapper(client, m, guild))
                    .ToList()
                    .AsReadOnly();
            });
        }

        public IRequestGuildMembersAction Request()
            => new RequestGuildMembersAction(manager._client, guildId);

        private IGuild ResolveGuild()
            => guildContext
               ?? manager._client.Guilds.GetWrapped(guildId)
               ?? throw new InvalidOperationException(
                   $"Guild {guildId} is not in the cache. The bot must have received GUILD_CREATE for this guild before its member REST surface can be used.");
    }
}

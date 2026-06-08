using DiscoSdk.Caching;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Presences;
using System.Collections.Concurrent;

namespace DiscoSdk.Hosting.Managers;

/// <summary>
/// Process-local cache of the latest <see cref="Presence"/> seen for each member, partitioned by
/// guild. The gateway dispatcher upserts entries from <c>PRESENCE_UPDATE</c> events and the
/// initial <c>GUILD_CREATE</c> presence snapshot; the cache is what
/// <see cref="Wrappers.GuildMemberWrapper"/> consults to surface
/// <see cref="IMember.OnlineStatus"/>, <see cref="IMember.GetOnlineStatus"/>,
/// <see cref="IMember.Activities"/>, and <see cref="IMember.ActiveClients"/>.
/// </summary>
internal sealed class PresenceManager
{
    /// <summary>Default flag set when the host does not register a custom value via DI.</summary>
    internal const PresenceCacheFlag DefaultFlags = PresenceCacheFlag.ClientStatus;

    private readonly ConcurrentDictionary<Snowflake, ConcurrentDictionary<Snowflake, Presence>> _byGuild = new();
    private readonly PresenceCacheFlag _flags;

    internal PresenceManager(PresenceCacheFlag flags = DefaultFlags)
    {
        _flags = flags;
    }

    internal PresenceCacheFlag Flags => _flags;

    /// <summary>
    /// Retrieves the latest known presence for a member as a read-only <see cref="IPresence"/>
    /// wrapper, or <c>null</c> when none has been observed yet. Returning the wrapper (instead of
    /// the raw POCO) ensures activities surface as <see cref="Activities.IActivity"/> entries and
    /// the underlying mutable model never leaks to consumers.
    /// </summary>
    internal IPresence? TryGet(Snowflake guildId, Snowflake userId)
    {
        if (guildId.Empty || userId.Empty)
            return null;

        if (!_byGuild.TryGetValue(guildId, out var guildCache))
            return null;

        return guildCache.TryGetValue(userId, out var presence) ? new PresenceWrapper(presence) : null;
    }

    /// <summary>
    /// Internal POCO accessor used by <see cref="GuildMemberWrapper"/> when it needs to read
    /// fields not surfaced through <see cref="IPresence"/> (such as per-client status flags).
    /// </summary>
    internal ClientStatus? TryGetClientStatus(Snowflake guildId, Snowflake userId)
    {
        if (guildId.Empty || userId.Empty)
            return null;

        if (!_byGuild.TryGetValue(guildId, out var guildCache))
            return null;

        return guildCache.TryGetValue(userId, out var presence) ? presence.ClientStatus : null;
    }

    /// <summary>
    /// Stores a <see cref="Presence"/> received from <c>PRESENCE_UPDATE</c>. When the status is
    /// <c>"offline"</c> the entry is dropped — Discord uses an offline event as a tombstone.
    /// </summary>
    internal void OnPresenceUpdate(Presence presence, Snowflake guildId)
    {
        if (_flags == PresenceCacheFlag.None)
            return;

        var userId = presence?.User?.Id ?? default;
        if (guildId.Empty || userId.Empty)
            return;

        if (IsOffline(presence!.Status))
        {
            if (_byGuild.TryGetValue(guildId, out var guildCache))
                guildCache.TryRemove(userId, out _);
            return;
        }

        GetOrAddGuildCache(guildId)[userId] = Filter(presence);
    }

    /// <summary>
    /// Bulk-seeds presences received in the <c>GUILD_CREATE</c> snapshot. Offline entries are
    /// skipped so the cache only retains members currently signalled as connected.
    /// </summary>
    internal void OnGuildPresencesSeed(IEnumerable<Presence>? presences, Snowflake guildId)
    {
        if (_flags == PresenceCacheFlag.None || presences is null || guildId.Empty)
            return;

        foreach (var presence in presences)
        {
            if (presence is null) continue;
            var userId = presence.User?.Id ?? default;
            if (userId.Empty) continue;
            if (IsOffline(presence.Status)) continue;

            GetOrAddGuildCache(guildId)[userId] = Filter(presence);
        }
    }

    /// <summary>
    /// Drops every cached presence for the supplied guild. Called when the bot leaves the guild
    /// or it becomes unavailable.
    /// </summary>
    internal void OnGuildRemove(Snowflake guildId)
    {
        if (!guildId.Empty)
            _byGuild.TryRemove(guildId, out _);
    }

    /// <summary>
    /// Maps the raw Discord status string to the SDK's <see cref="OnlineStatus"/> enum. Returns
    /// <c>null</c> when the string is missing or unrecognized so callers can fall back to a
    /// default presence representation.
    /// </summary>
    internal static OnlineStatus? MapStatus(string? status) => status switch
    {
        "online" => OnlineStatus.Online,
        "idle" => OnlineStatus.Idle,
        "dnd" => OnlineStatus.DoNotDisturb,
        "offline" => OnlineStatus.Offline,
        "invisible" => OnlineStatus.Invisible,
        _ => null
    };

    private static bool IsOffline(string? status)
        => string.Equals(status, "offline", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Strips fields that are not selected by <see cref="_flags"/> before the presence is stored.
    /// A defensive copy keeps callers' POCO instances untouched.
    /// </summary>
    private Presence Filter(Presence source)
    {
        var keepStatus = _flags.HasFlag(PresenceCacheFlag.ClientStatus);
        var keepActivities = _flags.HasFlag(PresenceCacheFlag.Activities);
        return new()
        {
            User = source.User,
            ProcessedAtTimestamp = source.ProcessedAtTimestamp,
            Status = keepStatus ? source.Status : null,
            ClientStatus = keepStatus ? source.ClientStatus : null,
            Activities = keepActivities ? source.Activities : [],
            Game = keepActivities ? source.Game : null
        };
    }

    private ConcurrentDictionary<Snowflake, Presence> GetOrAddGuildCache(Snowflake guildId)
        => _byGuild.GetOrAdd(guildId, static _ => new ConcurrentDictionary<Snowflake, Presence>());
}

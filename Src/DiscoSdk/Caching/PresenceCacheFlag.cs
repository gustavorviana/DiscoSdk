namespace DiscoSdk.Caching;

/// <summary>
/// Bitfield controlling which pieces of a Discord <c>PRESENCE_UPDATE</c> payload the SDK
/// persists in its in-memory presence cache. Selecting fewer flags reduces the cache footprint
/// without affecting what Discord sends — the
/// <see cref="Models.Enums.DiscordIntent.GuildPresences"/> intent still governs whether the
/// gateway delivers presence events at all.
/// </summary>
[Flags]
public enum PresenceCacheFlag
{
    /// <summary>Nothing is cached. Every presence read returns the default value.</summary>
    None = 0,

    /// <summary>
    /// Cache the aggregate and per-device status (top-level <c>status</c> + the
    /// <c>client_status</c> object). Required for <see cref="Models.IMember.OnlineStatus"/>,
    /// <see cref="Models.IMember.GetOnlineStatus"/>, and
    /// <see cref="Models.IMember.ActiveClients"/> to reflect real data — and for
    /// <see cref="DiscoSdk.Hosting.Caching.Policies.OnlinePolicy"/> to make accurate decisions.
    /// </summary>
    ClientStatus = 1 << 0,

    /// <summary>
    /// Cache the activity list (game + activities array). The largest source of presence churn —
    /// opt in only when the bot actually reads activities.
    /// </summary>
    Activities = 1 << 1,

    /// <summary>All presence fields are cached.</summary>
    All = ClientStatus | Activities
}

using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models;

/// <summary>
/// Per-guild ban surface — every operation that targets <c>/guilds/:id/bans/*</c>. Mirrors the
/// facade pattern used by <see cref="DiscoSdk.Caching.IGuildMembers"/> /
/// <see cref="DiscoSdk.Caching.IGuildStickers"/>: every method returns an
/// <see cref="IRestAction{T}"/> or specialized builder so callers compose with the rest of
/// <see cref="IGuild"/>. Bans are REST-only on the Discord API (no GUILD_BAN_*_BULK gateway
/// signal), so there is no cache layer here.
/// </summary>
public interface IGuildBans
{
    /// <summary>
    /// Builds a deferred action that retrieves a single ban by user id. Returns <c>null</c> when
    /// the user is not banned from this guild.
    /// </summary>
    IRestAction<IBan?> Get(Snowflake userId);

    /// <summary>
    /// Builds a deferred paginated REST action that lists this guild's bans.
    /// </summary>
    IBanPaginationAction List();

    /// <summary>
    /// Builds a deferred REST action that bans a user from this guild. Chain
    /// <see cref="IRestActionWithReason{TSelf}.WithReason"/> to attach an audit-log reason.
    /// </summary>
    /// <param name="userId">The ID of the user to ban.</param>
    /// <param name="deleteMessageDays">The number of days of messages to delete (0-7).</param>
    IBanMemberAction Ban(Snowflake userId, int deleteMessageDays = 0);

    /// <summary>
    /// Builds a deferred REST action that unbans a user from this guild. Chain
    /// <see cref="IRestActionWithReason{TSelf}.WithReason"/> to attach an audit-log reason.
    /// </summary>
    IReasonedRestAction Unban(Snowflake userId);

    /// <summary>
    /// Builds a deferred REST action that bans multiple users at once (up to 200 in a single call).
    /// Returns the ids of users that were successfully banned.
    /// </summary>
    /// <param name="userIds">The users to ban.</param>
    /// <param name="deleteMessageSeconds">If set, the number of seconds of recent message history to wipe (0 to 604 800 / 7 days).</param>
    IReasonedRestAction<IReadOnlyList<Snowflake>> BulkBan(IEnumerable<Snowflake> userIds, int? deleteMessageSeconds = null);
}

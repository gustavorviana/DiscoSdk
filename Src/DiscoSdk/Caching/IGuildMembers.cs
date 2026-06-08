using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Caching;

/// <summary>
/// Per-guild member surface. Every method returns an <see cref="IRestAction{T}"/> (or specialized
/// builder) so callers compose with the same deferred-execution pattern used everywhere else on
/// <see cref="IGuild"/>. Cache-aware operations short-circuit REST when the cache satisfies the
/// request; the deferred shape is preserved for symmetry. Access via
/// <see cref="DiscoSdk.Models.IGuild.Members"/>.
/// </summary>
public interface IGuildMembers
{
    /// <summary>
    /// Builds a deferred action that resolves a member by user id. Defaults to
    /// <see cref="MemberFetchMode.CacheThenRest"/> — cache first, REST on miss, write-back when the
    /// configured policy accepts the result. Use <see cref="MemberFetchMode.CacheOnly"/> to skip
    /// REST or <see cref="MemberFetchMode.RestOnly"/> to bypass the cache.
    /// </summary>
    /// <param name="userId">The user identifying the member.</param>
    /// <param name="mode">Controls the cache/REST traversal.</param>
    IRestAction<IMember?> Get(Snowflake userId, MemberFetchMode mode = MemberFetchMode.CacheThenRest);

    /// <summary>
    /// Enumerates every member currently in the cache for this guild. Reads come straight from
    /// the in-memory <c>ConcurrentDictionary</c> — synchronous, no I/O. Reflects the cache
    /// snapshot at iteration time; concurrent additions/removals may or may not be observed.
    /// </summary>
    IEnumerable<IMember> GetCached();

    /// <summary>
    /// Returns the number of members currently in the cache for this guild. Synchronous.
    /// </summary>
    int GetCachedCount();

    /// <summary>
    /// Builds a deferred paginated REST action that lists members of this guild.
    /// Requires the privileged <see cref="DiscordIntent.GuildMembers"/> intent — Discord's
    /// <c>List Guild Members</c> endpoint refuses to return data without it.
    /// </summary>
    /// <exception cref="DiscoSdk.Exceptions.MissingIntentException">
    /// Thrown at execution time when <see cref="DiscordIntent.GuildMembers"/> is not enabled on the client.
    /// </exception>
    IMemberPaginationAction List();

    /// <summary>
    /// Builds a deferred REST action that searches this guild's member list by username/nickname
    /// prefix.
    /// </summary>
    /// <param name="query">The username/nickname prefix to match. Required, non-blank.</param>
    /// <param name="limit">Maximum number of members to return (1–1000). Defaults server-side to 1.</param>
    IRestAction<IReadOnlyList<IMember>> Search(string query, int? limit = null);

    /// <summary>
    /// Builds a deferred REST action that adds a user to this guild using an OAuth2 access token
    /// granted with the <c>guilds.join</c> scope. Configure optional fields (nickname, roles, mute,
    /// deaf) on the returned builder, then call <see cref="IRestAction{T}.ExecuteAsync"/>. Returns
    /// <c>null</c> if the user was already in the guild.
    /// </summary>
    IAddMemberAction Add(Snowflake userId, string accessToken);

    /// <summary>
    /// Builds a deferred REST action that modifies a guild member. Each setter on the builder
    /// corresponds to one Discord field — only the fields you set are sent.
    /// Use <see cref="IModifyMemberAction.Timeout"/> / <see cref="IModifyMemberAction.ClearTimeout"/>
    /// instead of passing a sentinel timestamp.
    /// </summary>
    /// <param name="userId">The member to modify.</param>
    IModifyMemberAction Modify(Snowflake userId);

    /// <summary>
    /// Builds a deferred REST action that updates the bot's own nickname in this guild. Pass
    /// <c>null</c> to clear it.
    /// </summary>
    IReasonedRestAction<IMember> ModifyCurrent(string? nick);

    /// <summary>
    /// Builds a deferred REST action that adds a role to a guild member. Chain
    /// <see cref="IRestActionWithReason{TSelf}.WithReason"/> to attach an audit-log reason.
    /// </summary>
    IReasonedRestAction AddRole(Snowflake userId, Snowflake roleId);

    /// <summary>
    /// Builds a deferred REST action that removes a role from a guild member. Chain
    /// <see cref="IRestActionWithReason{TSelf}.WithReason"/> to attach an audit-log reason.
    /// </summary>
    IReasonedRestAction RemoveRole(Snowflake userId, Snowflake roleId);

    /// <summary>
    /// Builds a deferred REST action that kicks a member from this guild. Chain
    /// <see cref="IRestActionWithReason{TSelf}.WithReason"/> to attach an audit-log reason.
    /// Kicking removes the member from the guild without creating a ban — for permanent removal
    /// use <see cref="DiscoSdk.Models.IGuildBans.Ban"/> instead.
    /// </summary>
    IReasonedRestAction Kick(Snowflake userId);

    /// <summary>
    /// Builds a deferred Request Guild Members gateway action (op 8).
    /// </summary>
    /// <remarks>
    /// Intent requirements depend on how the action is configured at terminal time:
    /// <list type="bullet">
    /// <item>Empty query (full member list) requires <see cref="DiscordIntent.GuildMembers"/>.</item>
    /// <item><see cref="IRequestGuildMembersAction.SetPresences(bool)"/> with <c>true</c> requires <see cref="DiscordIntent.GuildPresences"/>.</item>
    /// <item>A non-empty <see cref="IRequestGuildMembersAction.SetQuery(string)"/> or explicit <see cref="IRequestGuildMembersAction.SetUserIds(Snowflake[])"/> require no extra intent.</item>
    /// </list>
    /// Missing intents throw <see cref="DiscoSdk.Exceptions.MissingIntentException"/> when the
    /// terminal <c>GetAsync</c> or <c>StreamAsync</c> is invoked, before any payload is sent.
    /// </remarks>
    IRequestGuildMembersAction Request();
}

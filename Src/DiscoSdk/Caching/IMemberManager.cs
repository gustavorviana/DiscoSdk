using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Caching;

/// <summary>
/// Cross-guild member access surface combining the cache, REST fallback, and the configured
/// <see cref="IMemberCachePolicy"/>.
/// </summary>
public interface IMemberManager
{
    /// <summary>
    /// Builds a deferred action that resolves a single member by composite identity. The default
    /// fetch mode hits the cache first and falls back to REST on miss, populating the cache when
    /// the policy accepts the result.
    /// </summary>
    /// <param name="guildId">The guild the member belongs to.</param>
    /// <param name="userId">The user identifying the member.</param>
    /// <param name="mode">Controls the cache/REST traversal.</param>
    IRestAction<IMember?> Get(
        Snowflake guildId,
        Snowflake userId,
        MemberFetchMode mode = MemberFetchMode.CacheThenRest);

    /// <summary>
    /// Returns a per-guild member surface that combines cache-aware reads, REST builders, and the
    /// gateway Request Guild Members (op 8) flow, all pre-bound to <paramref name="guildId"/>.
    /// </summary>
    /// <param name="guildId">The guild to scope to.</param>
    IGuildMembers OfGuild(Snowflake guildId);
}

using DiscoSdk.Models;

namespace DiscoSdk.Caching;

/// <summary>
/// Cross-guild member access surface combining the cache, REST fallback, and the configured
/// <see cref="IMemberCachePolicy"/>.
/// </summary>
public interface IMemberManager
{
    /// <summary>
    /// Looks up a single member by composite identity. The default fetch mode hits the cache first
    /// and falls back to REST on miss, populating the cache when the policy accepts the result.
    /// </summary>
    /// <param name="guildId">The guild the member belongs to.</param>
    /// <param name="userId">The user identifying the member.</param>
    /// <param name="mode">Controls the cache/REST traversal.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The member when found; <c>null</c> when the member does not exist or is unavailable.</returns>
    ValueTask<IMember?> GetAsync(
        Snowflake guildId,
        Snowflake userId,
        MemberFetchMode mode = MemberFetchMode.CacheThenRest,
        CancellationToken ct = default);

    /// <summary>
    /// Returns a per-guild member surface that combines cache-aware reads, REST builders, and the
    /// gateway Request Guild Members (op 8) flow, all pre-bound to <paramref name="guildId"/>.
    /// </summary>
    /// <param name="guildId">The guild to scope to.</param>
    IGuildMembers OfGuild(Snowflake guildId);
}

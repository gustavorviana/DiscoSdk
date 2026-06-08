using DiscoSdk.Models;

namespace DiscoSdk.Caching;

/// <summary>
/// Per-guild member access surface. Combines cached lookups, REST fallback, and iteration over
/// the members currently held in the cache for a given guild.
/// </summary>
public interface IGuildMemberScope
{
    /// <summary>The guild this scope is bound to.</summary>
    Snowflake GuildId { get; }

    /// <summary>
    /// Looks up a single member inside this guild. See <see cref="IMemberManager.GetAsync"/> for
    /// fetch-mode semantics.
    /// </summary>
    /// <param name="userId">The user identifying the member.</param>
    /// <param name="mode">Controls the cache/REST traversal.</param>
    /// <param name="ct">A cancellation token.</param>
    ValueTask<IMember?> GetAsync(
        Snowflake userId,
        MemberFetchMode mode = MemberFetchMode.CacheThenRest,
        CancellationToken ct = default);

    /// <summary>
    /// Enumerates every member currently present in the cache for this guild. The result reflects
    /// the cache snapshot at iteration time; entries may be added or removed concurrently.
    /// </summary>
    /// <param name="ct">A cancellation token.</param>
    IAsyncEnumerable<IMember> GetCachedAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns the number of members currently present in the cache for this guild.
    /// </summary>
    /// <param name="ct">A cancellation token.</param>
    ValueTask<int> GetCachedCountAsync(CancellationToken ct = default);
}

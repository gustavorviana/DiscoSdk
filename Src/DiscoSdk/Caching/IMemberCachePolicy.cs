using DiscoSdk.Models;

namespace DiscoSdk.Caching;

/// <summary>
/// Predicate that decides whether a given member should be cached by the SDK.
/// </summary>
/// <remarks>
/// Policies are pure predicates: they inspect the member and return a boolean decision.
/// The SDK re-evaluates the policy on every gateway event that may change the outcome
/// (member updates, voice state changes, presence changes, role changes) and upserts or evicts
/// the cache entry accordingly. Implementations must be thread-safe and free of side effects.
/// </remarks>
public interface IMemberCachePolicy
{
    /// <summary>
    /// Returns <c>true</c> when the supplied member should be present in the cache.
    /// </summary>
    /// <param name="member">The member to evaluate.</param>
    bool ShouldCache(IMember member);
}

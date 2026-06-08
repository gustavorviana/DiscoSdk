namespace DiscoSdk.Caching;

/// <summary>
/// Controls how a member lookup traverses the cache and the REST fallback.
/// </summary>
public enum MemberFetchMode
{
    /// <summary>
    /// Look up the member in the cache only. Returns <c>null</c> when the member is not present,
    /// without performing any REST request.
    /// </summary>
    CacheOnly,

    /// <summary>
    /// Look up the member in the cache first and fall back to REST when the cache misses.
    /// The REST result is written back to the cache when the configured policy accepts it.
    /// </summary>
    CacheThenRest,

    /// <summary>
    /// Skip the cache entirely and fetch the member from REST. The REST result is written back to
    /// the cache when the configured policy accepts it.
    /// </summary>
    RestOnly
}

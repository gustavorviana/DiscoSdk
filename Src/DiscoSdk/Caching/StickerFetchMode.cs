namespace DiscoSdk.Caching;

/// <summary>
/// Controls how a sticker lookup traverses the SDK cache and the REST fallback.
/// </summary>
public enum StickerFetchMode
{
    /// <summary>
    /// Look up the sticker in the cache only. Returns <c>null</c> / an empty list when the
    /// cache does not have the requested data, without performing any REST request.
    /// </summary>
    CacheOnly,

    /// <summary>
    /// Look up the sticker in the cache first and fall back to REST when the cache misses.
    /// The REST result is written back to the cache when sticker caching is enabled.
    /// </summary>
    CacheThenRest,

    /// <summary>
    /// Skip the cache entirely and fetch the sticker from REST. The REST result is written back
    /// to the cache when sticker caching is enabled.
    /// </summary>
    RestOnly
}

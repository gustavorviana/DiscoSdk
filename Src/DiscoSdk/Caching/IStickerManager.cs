using DiscoSdk.Models;

namespace DiscoSdk.Caching;

/// <summary>
/// Cross-guild sticker access surface. Combines the in-memory sticker cache, REST fallback, and
/// per-guild scoping (<see cref="OfGuild"/>) for iteration. Every sticker read on the SDK goes
/// through this surface — there is no parallel REST-only entry point on <see cref="IGuild"/>.
/// </summary>
public interface IStickerManager
{
    /// <summary>
    /// Looks up a single guild sticker by composite identity. See
    /// <see cref="StickerFetchMode"/> for traversal semantics.
    /// </summary>
    /// <param name="guildId">The guild that owns the sticker.</param>
    /// <param name="stickerId">The sticker id.</param>
    /// <param name="mode">Controls the cache / REST traversal.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The sticker when found; <c>null</c> when it does not exist or is unavailable.</returns>
    ValueTask<ISticker?> GetAsync(
        Snowflake guildId,
        Snowflake stickerId,
        StickerFetchMode mode = StickerFetchMode.CacheThenRest,
        CancellationToken ct = default);

    /// <summary>
    /// Returns a scope bound to the supplied guild. The scope reuses this manager and the same
    /// underlying cache, just pre-bound so callers don't repeat the guild id.
    /// </summary>
    IGuildStickerScope OfGuild(Snowflake guildId);
}

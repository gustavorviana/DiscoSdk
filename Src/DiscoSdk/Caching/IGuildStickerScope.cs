using DiscoSdk.Models;

namespace DiscoSdk.Caching;

/// <summary>
/// Per-guild sticker access surface. Combines cache reads, REST fallback, and a synchronous
/// cache snapshot.
/// </summary>
public interface IGuildStickerScope
{
    /// <summary>The guild this scope is bound to.</summary>
    Snowflake GuildId { get; }

    /// <summary>
    /// Looks up a single sticker in this guild. See <see cref="IStickerManager.GetAsync"/> for
    /// fetch-mode semantics.
    /// </summary>
    /// <param name="stickerId">The sticker id.</param>
    /// <param name="mode">Controls the cache / REST traversal.</param>
    /// <param name="ct">A cancellation token.</param>
    ValueTask<ISticker?> GetAsync(
        Snowflake stickerId,
        StickerFetchMode mode = StickerFetchMode.CacheThenRest,
        CancellationToken ct = default);

    /// <summary>
    /// Returns every sticker available for this guild. Cache-aware semantics follow
    /// <paramref name="mode"/>; with <see cref="StickerFetchMode.CacheThenRest"/> the cache is
    /// preferred and REST only runs when the cache is empty.
    /// </summary>
    /// <param name="mode">Controls the cache / REST traversal.</param>
    /// <param name="ct">A cancellation token.</param>
    ValueTask<IReadOnlyList<ISticker>> GetAllAsync(
        StickerFetchMode mode = StickerFetchMode.CacheThenRest,
        CancellationToken ct = default);

    /// <summary>
    /// Returns a snapshot of the stickers currently held in the cache for this guild. Empty when
    /// sticker caching is disabled or the bot has not received <c>GUILD_CREATE</c> /
    /// <c>GUILD_STICKERS_UPDATE</c> for the guild yet.
    /// </summary>
    IReadOnlyList<ISticker> GetCached();
}

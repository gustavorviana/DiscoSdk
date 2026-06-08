using DiscoSdk.Models;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Caching;

/// <summary>
/// Per-guild sticker surface. Mirrors the shape of <see cref="IGuildMembers"/>: every method that
/// can touch the network returns an <see cref="IRestAction{T}"/> (or specialized builder) so it
/// composes with the rest of <see cref="IGuild"/>, while pure cache reads stay synchronous —
/// stickers are kept in a process-local <c>ConcurrentDictionary</c>, no I/O to await. Access via
/// <see cref="DiscoSdk.Models.IGuild.Stickers"/>.
/// </summary>
public interface IGuildStickers
{
    /// <summary>
    /// Builds a deferred action that resolves a single sticker by id. Defaults to
    /// <see cref="StickerFetchMode.CacheThenRest"/>; switch to <see cref="StickerFetchMode.CacheOnly"/>
    /// to skip REST or <see cref="StickerFetchMode.RestOnly"/> to bypass the cache. The REST result
    /// is written back to the cache when sticker caching is enabled.
    /// </summary>
    /// <param name="stickerId">The sticker id.</param>
    /// <param name="mode">Controls the cache / REST traversal.</param>
    IRestAction<ISticker?> Get(Snowflake stickerId, StickerFetchMode mode = StickerFetchMode.CacheThenRest);

    /// <summary>
    /// Builds a deferred action that returns every sticker available for this guild. With
    /// <see cref="StickerFetchMode.CacheThenRest"/> the cache is preferred and REST only runs when
    /// the cache is empty.
    /// </summary>
    /// <param name="mode">Controls the cache / REST traversal.</param>
    IRestAction<IReadOnlyList<ISticker>> GetAll(StickerFetchMode mode = StickerFetchMode.CacheThenRest);

    /// <summary>
    /// Synchronous snapshot of every sticker currently in the cache for this guild. Empty when
    /// sticker caching is disabled or the bot has not received <c>GUILD_CREATE</c> /
    /// <c>GUILD_STICKERS_UPDATE</c> for the guild yet. Reads come straight from the in-memory
    /// store — no I/O.
    /// </summary>
    IReadOnlyList<ISticker> GetCached();

    /// <summary>
    /// Synchronous count of stickers currently in the cache for this guild.
    /// </summary>
    int GetCachedCount();

    /// <summary>
    /// Builds a deferred REST action that uploads a new sticker to this guild. Chain
    /// <c>SetDescription(...)</c> for an optional description, then <c>ExecuteAsync</c>.
    /// </summary>
    /// <param name="name">Sticker name (2-30 chars).</param>
    /// <param name="tags">Suggestion / AutoComplete tag string (max 200 chars).</param>
    /// <param name="file">Sticker image file (PNG/APNG/GIF/Lottie, max 512 KiB).</param>
    ICreateGuildStickerAction Create(string name, string tags, MessageFile file);
}

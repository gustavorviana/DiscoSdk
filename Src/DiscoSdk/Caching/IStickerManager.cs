using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Caching;

/// <summary>
/// Cross-guild sticker access surface. Combines the in-memory sticker cache, REST fallback, and
/// per-guild scoping (<see cref="OfGuild"/>) for iteration. Every sticker read on the SDK goes
/// through this surface — there is no parallel REST-only entry point on <see cref="IGuild"/>.
/// </summary>
public interface IStickerManager
{
    /// <summary>
    /// Builds a deferred action that resolves a single guild sticker by composite identity. See
    /// <see cref="StickerFetchMode"/> for traversal semantics.
    /// </summary>
    /// <param name="guildId">The guild that owns the sticker.</param>
    /// <param name="stickerId">The sticker id.</param>
    /// <param name="mode">Controls the cache / REST traversal.</param>
    IRestAction<ISticker?> Get(
        Snowflake guildId,
        Snowflake stickerId,
        StickerFetchMode mode = StickerFetchMode.CacheThenRest);

    /// <summary>
    /// Returns a per-guild sticker surface combining cache reads, REST builders, and the
    /// <c>Create</c> upload action, all pre-bound to <paramref name="guildId"/>.
    /// </summary>
    IGuildStickers OfGuild(Snowflake guildId);
}

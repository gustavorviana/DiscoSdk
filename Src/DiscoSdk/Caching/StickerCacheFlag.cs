namespace DiscoSdk.Caching;

/// <summary>
/// Controls whether the SDK keeps an in-memory copy of the stickers a guild has uploaded. The
/// flag is independent from the <see cref="Models.Enums.DiscordIntent.GuildExpressions"/> intent
/// — without that intent Discord does not deliver <c>GUILD_STICKERS_UPDATE</c> events and the
/// only data the cache ever sees is the snapshot included in <c>GUILD_CREATE</c>.
/// </summary>
public enum StickerCacheFlag
{
    /// <summary>No stickers are cached. <see cref="IGuildStickers.GetCached"/> is always empty.</summary>
    None = 0,

    /// <summary>
    /// Cache the stickers each guild has uploaded. Standard pack stickers stay REST-only — the
    /// SDK never caches them. Off by default; bots that read stickers from cached guild data
    /// should opt in.
    /// </summary>
    Guild = 1
}

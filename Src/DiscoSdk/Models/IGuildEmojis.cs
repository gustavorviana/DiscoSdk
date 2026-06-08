using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models;

/// <summary>
/// Per-guild emoji surface — every operation that targets <c>/guilds/:id/emojis*</c> plus the
/// cached snapshot fed by the gateway (<c>GUILD_CREATE</c> / <c>GUILD_EMOJIS_UPDATE</c>).
/// </summary>
public interface IGuildEmojis
{
    /// <summary>
    /// Builds a deferred REST action that creates a new custom emoji in this guild.
    /// </summary>
    /// <param name="name">The emoji name.</param>
    /// <param name="image">The emoji image data.</param>
    ICreateEmojiAction Create(string name, DiscordImageBuffer image);

    /// <summary>
    /// Synchronous snapshot of every emoji currently held in the cache for this guild. Reads come
    /// straight from the in-memory store fed by gateway events — no I/O.
    /// </summary>
    IReadOnlyList<IEmoji> GetCached();

    /// <summary>Synchronous count of emojis currently in the cache for this guild.</summary>
    int GetCachedCount();
}

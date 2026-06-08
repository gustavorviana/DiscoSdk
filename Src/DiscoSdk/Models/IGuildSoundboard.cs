using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models;

/// <summary>
/// Per-guild soundboard surface — every operation that targets
/// <c>/guilds/:id/soundboard-sounds*</c>. Discord-built-in default sounds are read-only and live
/// on a separate global endpoint that is not part of this surface.
/// </summary>
/// <remarks>
/// Cache-population gateway events (<c>GUILD_SOUNDBOARD_SOUND_CREATE</c> / <c>UPDATE</c> /
/// <c>DELETE</c>) require <see cref="DiscoSdk.Models.Enums.DiscordIntent.GuildExpressions"/>. The
/// REST endpoints below do not — Discord gates them on the
/// <see cref="DiscoSdk.Models.Enums.DiscordPermission.CreateGuildExpressions"/> /
/// <see cref="DiscoSdk.Models.Enums.DiscordPermission.ManageEmojisAndStickers"/> permissions
/// instead.
/// </remarks>
public interface IGuildSoundboard
{
    /// <summary>Builds a deferred REST action that lists every soundboard sound owned by this guild.</summary>
    IRestAction<IReadOnlyList<ISoundboardSound>> GetAll();

    /// <summary>Builds a deferred REST action that retrieves a single soundboard sound by id.</summary>
    IRestAction<ISoundboardSound> Get(Snowflake soundId);

    /// <summary>
    /// Builds a deferred fluent action that creates a soundboard sound on this guild. Supply the
    /// audio buffer (MP3/OGG, ≤ 512 KiB, ≤ 5.2 s) and the display name; chain <c>SetVolume(...)</c>
    /// / <c>SetEmoji(...)</c> on the builder as needed.
    /// </summary>
    /// <param name="name">Display name (2–32 chars).</param>
    /// <param name="sound">Audio payload. See <see cref="DiscordSoundBuffer"/> for format limits.</param>
    ICreateSoundboardSoundAction Create(string name, DiscordSoundBuffer sound);
}

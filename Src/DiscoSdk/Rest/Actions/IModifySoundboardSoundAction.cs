using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Fluent builder for <c>PATCH /guilds/:id/soundboard-sounds/:sound.id</c>. Only the fields
/// touched on the builder are sent on the wire; the rest are preserved on Discord's side.
/// </summary>
/// <remarks>
/// Bot needs <see cref="DiscordPermission.ManageEmojisAndStickers"/> for sounds it did not
/// upload itself, and <see cref="DiscordPermission.CreateGuildExpressions"/> for sounds it
/// owns. Discord enforces both — the SDK does not pre-check.
/// </remarks>
public interface IModifySoundboardSoundAction : IRestAction<ISoundboardSound>
{
    /// <summary>Renames the sound (2–32 chars).</summary>
    IModifySoundboardSoundAction SetName(string name);

    /// <summary>
    /// Updates the playback volume (range <c>[0, 1]</c>).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="volume"/> is outside <c>[0, 1]</c>.</exception>
    IModifySoundboardSoundAction SetVolume(double volume);

    /// <summary>Binds the sound to a custom guild emoji. Mutually exclusive with <see cref="SetEmoji(string)"/>.</summary>
    IModifySoundboardSoundAction SetEmoji(Snowflake emojiId);

    /// <summary>Binds the sound to a Unicode emoji. Mutually exclusive with <see cref="SetEmoji(Snowflake)"/>.</summary>
    IModifySoundboardSoundAction SetEmoji(string unicodeEmoji);

    /// <summary>
    /// Clears any emoji binding from the sound — sends explicit <c>null</c> for both
    /// <c>emoji_id</c> and <c>emoji_name</c> so Discord forgets the current binding.
    /// </summary>
    IModifySoundboardSoundAction ClearEmoji();
}

using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Fluent builder for <c>POST /guilds/:id/soundboard-sounds</c>. The required fields
/// (<c>name</c> and the audio buffer) are supplied at construction time; everything else is
/// optional and only sent when the matching <c>Set…</c> is invoked.
/// </summary>
/// <remarks>
/// Discord's portal cap on guild soundboard sounds depends on the guild's premium tier
/// (8 / 24 / 36 / 48 for tiers 0–3). The bot needs the
/// <see cref="DiscordPermission.CreateGuildExpressions"/> permission. The audio buffer must be
/// MP3 or OGG, ≤ 512 KiB, ≤ 5.2 s; see <see cref="DiscordSoundBuffer"/>.
/// </remarks>
public interface ICreateSoundboardSoundAction : IRestAction<ISoundboardSound>
{
    /// <summary>
    /// Sets the playback volume in the closed range <c>[0, 1]</c>. Default at Discord is
    /// <c>1.0</c> when the field is omitted.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="volume"/> is outside <c>[0, 1]</c>.</exception>
    ICreateSoundboardSoundAction SetVolume(double volume);

    /// <summary>Binds the sound to a custom guild emoji. Mutually exclusive with <see cref="SetEmoji(string)"/>.</summary>
    ICreateSoundboardSoundAction SetEmoji(Snowflake emojiId);

    /// <summary>Binds the sound to a Unicode emoji (e.g. <c>"🔥"</c>). Mutually exclusive with <see cref="SetEmoji(Snowflake)"/>.</summary>
    ICreateSoundboardSoundAction SetEmoji(string unicodeEmoji);

    /// <summary>Replaces the audio buffer supplied at construction.</summary>
    ICreateSoundboardSoundAction SetSound(DiscordSoundBuffer sound);

    /// <summary>Replaces the name supplied at construction (2–32 chars).</summary>
    ICreateSoundboardSoundAction SetName(string name);
}

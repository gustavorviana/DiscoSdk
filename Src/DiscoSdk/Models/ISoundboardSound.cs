using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models;

/// <summary>
/// Public surface for a Discord guild soundboard sound — a short audio clip that members
/// can play into a voice channel. Modify / Delete are valid for guild-owned sounds only;
/// Discord's built-in default sounds (listed via the global default-sounds endpoint) are
/// read-only.
/// </summary>
/// <remarks>
/// Receiving the SDK's soundboard cache-population events (<c>GUILD_SOUNDBOARD_SOUND_CREATE</c>
/// / <c>UPDATE</c> / <c>DELETE</c>) requires the <see cref="DiscordIntent.GuildExpressions"/>
/// gateway intent. The REST endpoints exposed here do <em>not</em> require the intent — Discord
/// gates them on the <c>CREATE_GUILD_EXPRESSIONS</c> / <c>MANAGE_GUILD_EXPRESSIONS</c>
/// permission instead.
/// </remarks>
public interface ISoundboardSound : IWithSnowflake
{
    /// <summary>Display name of the sound (2–32 chars).</summary>
    string Name { get; }

    /// <summary>Playback volume in the closed range <c>[0, 1]</c>. Default is <c>1</c>.</summary>
    double Volume { get; }

    /// <summary>
    /// Snowflake of the custom guild emoji bound to the sound, or <c>null</c> when the sound
    /// uses a Unicode emoji (see <see cref="EmojiName"/>) or no emoji at all.
    /// </summary>
    Snowflake? EmojiId { get; }

    /// <summary>
    /// Unicode emoji bound to the sound, or <c>null</c> when the sound uses a custom guild
    /// emoji (see <see cref="EmojiId"/>) or no emoji at all.
    /// </summary>
    string? EmojiName { get; }

    /// <summary>
    /// Snowflake of the guild that owns the sound, or <c>null</c> for Discord's built-in
    /// default sounds.
    /// </summary>
    Snowflake? GuildId { get; }

    /// <summary>
    /// Whether the sound is currently usable. Goes <c>false</c> if the owning guild loses its
    /// premium tier and the sound is over the free-tier sound quota.
    /// </summary>
    bool Available { get; }

    /// <summary>
    /// Snowflake of the user who uploaded the sound, or <c>null</c> when the calling bot
    /// lacks <c>MANAGE_GUILD_EXPRESSIONS</c> (Discord redacts the uploader otherwise) or the
    /// sound is a Discord-built-in default.
    /// </summary>
    Snowflake? UserId { get; }

    /// <summary>
    /// Returns a fluent builder for modifying the sound (name / volume / emoji). Only fields
    /// touched on the builder are sent on the wire. Throws <see cref="InvalidOperationException"/>
    /// for default Discord sounds, which are read-only.
    /// </summary>
    IModifySoundboardSoundAction Modify();

    /// <summary>
    /// Deletes the sound from the owning guild. Throws <see cref="InvalidOperationException"/>
    /// for default Discord sounds, which are read-only.
    /// </summary>
    IRestAction Delete();
}

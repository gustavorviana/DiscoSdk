using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a soundboard sound in a Discord guild.
/// </summary>
public class SoundboardSound
{
    /// <summary>
    /// Gets or sets the volume of the sound.
    /// </summary>
    [JsonPropertyName("volume")]
    public double Volume { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who created the sound.
    /// </summary>
    [JsonPropertyName("user_id")]
    public Snowflake UserId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the ID of the sound.
    /// </summary>
    [JsonPropertyName("sound_id")]
    public Snowflake SoundId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the name of the sound.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the ID of the guild.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public Snowflake GuildId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the emoji name associated with the sound.
    /// </summary>
    [JsonPropertyName("emoji_name")]
    public string? EmojiName { get; set; }

    /// <summary>
    /// Gets or sets the ID of the emoji associated with the sound.
    /// </summary>
    [JsonPropertyName("emoji_id")]
    public Snowflake? EmojiId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the sound is available.
    /// </summary>
    [JsonPropertyName("available")]
    public bool Available { get; set; }
}


using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a voice state of a user in a Discord guild.
/// </summary>
public class VoiceState
{
    /// <summary>
    /// Gets or sets the guild ID for the voice state.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public Snowflake? GuildId { get; set; }

    /// <summary>
    /// Gets or sets the channel ID for the voice state.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public Snowflake? ChannelId { get; set; }

    /// <summary>
    /// Gets or sets the user ID for the voice state.
    /// </summary>
    [JsonPropertyName("user_id")]
    public Snowflake UserId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the member object for the voice state.
    /// </summary>
    [JsonPropertyName("member")]
    public GuildMember? Member { get; set; }

    /// <summary>
    /// Gets or sets the session ID for the voice state.
    /// </summary>
    [JsonPropertyName("session_id")]
    public string SessionId { get; set; } = default!;

    /// <summary>
    /// Gets or sets a value indicating whether the user is deafened.
    /// </summary>
    [JsonPropertyName("deaf")]
    public bool Deaf { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is muted.
    /// </summary>
    [JsonPropertyName("mute")]
    public bool Mute { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is self-deafened.
    /// </summary>
    [JsonPropertyName("self_deaf")]
    public bool SelfDeaf { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is self-muted.
    /// </summary>
    [JsonPropertyName("self_mute")]
    public bool SelfMute { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is streaming.
    /// </summary>
    [JsonPropertyName("self_stream")]
    public bool? SelfStream { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user's camera is enabled.
    /// </summary>
    [JsonPropertyName("self_video")]
    public bool SelfVideo { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is suppressed.
    /// </summary>
    [JsonPropertyName("suppress")]
    public bool Suppress { get; set; }

    /// <summary>
    /// Gets or sets the request to speak timestamp.
    /// </summary>
    [JsonPropertyName("request_to_speak_timestamp")]
    public string? RequestToSpeakTimestamp { get; set; }
}


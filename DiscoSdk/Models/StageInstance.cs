using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a stage instance in a Discord guild.
/// </summary>
public class StageInstance
{
    /// <summary>
    /// Gets or sets the ID of the stage instance.
    /// </summary>
    [JsonPropertyName("id")]
    public Snowflake Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the ID of the guild.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public Snowflake GuildId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the ID of the channel.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public Snowflake ChannelId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the topic of the stage instance.
    /// </summary>
    [JsonPropertyName("topic")]
    public string Topic { get; set; } = default!;

    /// <summary>
    /// Gets or sets the privacy level.
    /// </summary>
    [JsonPropertyName("privacy_level")]
    public int PrivacyLevel { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether discovery is disabled.
    /// </summary>
    [JsonPropertyName("discoverable_disabled")]
    public bool DiscoverableDisabled { get; set; }

    /// <summary>
    /// Gets or sets the ID of the scheduled event.
    /// </summary>
    [JsonPropertyName("guild_scheduled_event_id")]
    public Snowflake? GuildScheduledEventId { get; set; }
}


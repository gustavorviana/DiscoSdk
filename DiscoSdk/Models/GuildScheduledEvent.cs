using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a scheduled event in a Discord guild.
/// </summary>
public class GuildScheduledEvent
{
    /// <summary>
    /// Gets or sets the ID of the scheduled event.
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
    public Snowflake? ChannelId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who created the event.
    /// </summary>
    [JsonPropertyName("creator_id")]
    public Snowflake? CreatorId { get; set; }

    /// <summary>
    /// Gets or sets the name of the scheduled event.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the description of the scheduled event.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the scheduled start time.
    /// </summary>
    [JsonPropertyName("scheduled_start_time")]
    public string ScheduledStartTime { get; set; } = default!;

    /// <summary>
    /// Gets or sets the scheduled end time.
    /// </summary>
    [JsonPropertyName("scheduled_end_time")]
    public string? ScheduledEndTime { get; set; }

    /// <summary>
    /// Gets or sets the privacy level.
    /// </summary>
    [JsonPropertyName("privacy_level")]
    public int PrivacyLevel { get; set; }

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    [JsonPropertyName("status")]
    public int Status { get; set; }

    /// <summary>
    /// Gets or sets the entity type.
    /// </summary>
    [JsonPropertyName("entity_type")]
    public int EntityType { get; set; }

    /// <summary>
    /// Gets or sets the entity ID.
    /// </summary>
    [JsonPropertyName("entity_id")]
    public Snowflake? EntityId { get; set; }

    /// <summary>
    /// Gets or sets the entity metadata.
    /// </summary>
    [JsonPropertyName("entity_metadata")]
    public object? EntityMetadata { get; set; }

    /// <summary>
    /// Gets or sets the creator.
    /// </summary>
    [JsonPropertyName("creator")]
    public User? Creator { get; set; }

    /// <summary>
    /// Gets or sets the user count.
    /// </summary>
    [JsonPropertyName("user_count")]
    public int? UserCount { get; set; }

    /// <summary>
    /// Gets or sets the image.
    /// </summary>
    [JsonPropertyName("image")]
    public string? Image { get; set; }
}


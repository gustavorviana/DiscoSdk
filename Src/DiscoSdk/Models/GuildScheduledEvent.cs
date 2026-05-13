using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Users;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Raw model returned by Discord for a guild scheduled event.
/// </summary>
public class GuildScheduledEvent
{
    /// <summary>The scheduled event id.</summary>
    [JsonPropertyName("id")]
    public Snowflake Id { get; set; } = default!;

    /// <summary>The guild this event belongs to.</summary>
    [JsonPropertyName("guild_id")]
    public Snowflake GuildId { get; set; } = default!;

    /// <summary>
    /// The channel the event runs in (stage or voice). Null when the event's entity type is
    /// <see cref="ScheduledEventEntityType.External"/>.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public Snowflake? ChannelId { get; set; }

    /// <summary>The user that created the event. Null on events created before this field shipped.</summary>
    [JsonPropertyName("creator_id")]
    public Snowflake? CreatorId { get; set; }

    /// <summary>Event name (1-100 chars).</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>Event description (0-1000 chars).</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>When the event starts (ISO 8601).</summary>
    [JsonPropertyName("scheduled_start_time")]
    public DateTimeOffset ScheduledStartTime { get; set; }

    /// <summary>When the event ends. Required for External events; optional for Stage/Voice.</summary>
    [JsonPropertyName("scheduled_end_time")]
    public DateTimeOffset? ScheduledEndTime { get; set; }

    /// <summary>Privacy level.</summary>
    [JsonPropertyName("privacy_level")]
    public ScheduledEventPrivacyLevel PrivacyLevel { get; set; }

    /// <summary>Lifecycle status.</summary>
    [JsonPropertyName("status")]
    public ScheduledEventStatus Status { get; set; }

    /// <summary>Venue type (stage / voice / external).</summary>
    [JsonPropertyName("entity_type")]
    public ScheduledEventEntityType EntityType { get; set; }

    /// <summary>If the event is bound to a stage instance, its id.</summary>
    [JsonPropertyName("entity_id")]
    public Snowflake? EntityId { get; set; }

    /// <summary>Extra venue info (currently just location for External events).</summary>
    [JsonPropertyName("entity_metadata")]
    public ScheduledEventEntityMetadata? EntityMetadata { get; set; }

    /// <summary>The creator user. Null on events created before this field shipped.</summary>
    [JsonPropertyName("creator")]
    public User? Creator { get; set; }

    /// <summary>Number of users interested in the event (only present when requested).</summary>
    [JsonPropertyName("user_count")]
    public int? UserCount { get; set; }

    /// <summary>Cover image hash.</summary>
    [JsonPropertyName("image")]
    public string? Image { get; set; }
}

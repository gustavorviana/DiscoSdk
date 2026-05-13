using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Requests.ScheduledEvents;

/// <summary>
/// Request body for <c>PATCH /guilds/{guild.id}/scheduled-events/{event.id}</c>.
/// Reference: https://discord.com/developers/docs/resources/guild-scheduled-event#modify-guild-scheduled-event
/// <para>All fields are optional — only those set will be patched.</para>
/// </summary>
internal class ModifyScheduledEventRequest
{
	/// <summary>New event name.</summary>
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	/// <summary>New description.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>New start time.</summary>
	[JsonPropertyName("scheduled_start_time")]
	public DateTimeOffset? ScheduledStartTime { get; set; }

	/// <summary>New end time.</summary>
	[JsonPropertyName("scheduled_end_time")]
	public DateTimeOffset? ScheduledEndTime { get; set; }

	/// <summary>New privacy level.</summary>
	[JsonPropertyName("privacy_level")]
	public ScheduledEventPrivacyLevel? PrivacyLevel { get; set; }

	/// <summary>
	/// New lifecycle status. Allowed transitions: Scheduled → Active, Active → Completed,
	/// Scheduled → Canceled. Discord rejects illegal transitions.
	/// </summary>
	[JsonPropertyName("status")]
	public ScheduledEventStatus? Status { get; set; }

	/// <summary>New venue type.</summary>
	[JsonPropertyName("entity_type")]
	public ScheduledEventEntityType? EntityType { get; set; }

	/// <summary>New channel id (or null to detach for External events).</summary>
	[JsonPropertyName("channel_id")]
	public string? ChannelId { get; set; }

	/// <summary>New venue metadata.</summary>
	[JsonPropertyName("entity_metadata")]
	public ScheduledEventEntityMetadata? EntityMetadata { get; set; }

	/// <summary>New cover image.</summary>
	[JsonPropertyName("image")]
	public string? Image { get; set; }
}

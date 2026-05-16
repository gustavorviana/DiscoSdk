using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Models.Requests.ScheduledEvents;

/// <summary>
/// Request body for <c>POST /guilds/{guild.id}/scheduled-events</c>.
/// Reference: https://discord.com/developers/docs/resources/guild-scheduled-event#create-guild-scheduled-event
/// </summary>
internal class CreateScheduledEventRequest
{
	/// <summary>Event name (1-100 chars).</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = default!;

	/// <summary>When the event starts (ISO 8601).</summary>
	[JsonPropertyName("scheduled_start_time")]
	public DateTimeOffset ScheduledStartTime { get; set; }

	/// <summary>Venue type.</summary>
	[JsonPropertyName("entity_type")]
	public ScheduledEventEntityType EntityType { get; set; }

	/// <summary>Privacy level. Discord requires <c>GuildOnly</c>.</summary>
	[JsonPropertyName("privacy_level")]
	public ScheduledEventPrivacyLevel PrivacyLevel { get; set; }

	/// <summary>Channel id (required for Stage / Voice events).</summary>
	[JsonPropertyName("channel_id")]
	public string? ChannelId { get; set; }

	/// <summary>Event description (0-1000 chars).</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>Required for External events; optional for Stage/Voice.</summary>
	[JsonPropertyName("scheduled_end_time")]
	public DateTimeOffset? ScheduledEndTime { get; set; }

	/// <summary>Extra venue info (required <c>Location</c> for External events).</summary>
	[JsonPropertyName("entity_metadata")]
	public ScheduledEventEntityMetadata? EntityMetadata { get; set; }

	/// <summary>Image data URI (PNG/JPEG/GIF).</summary>
	[JsonPropertyName("image")]
	public string? Image { get; set; }
}

using System.Text.Json.Serialization;

namespace DiscoSdk.Models.AutoModeration;

/// <summary>
/// Additional data used when executing an <see cref="AutoModerationAction"/>. Which fields are present
/// depends on the action's <see cref="AutoModerationAction.Type"/>. Doubles as the public read surface
/// (<see cref="IAutoModerationActionMetadata"/>).
/// </summary>
public class AutoModerationActionMetadata : IAutoModerationActionMetadata
{
	/// <summary>Channel to which matched content should be logged (SEND_ALERT_MESSAGE).</summary>
	[JsonPropertyName("channel_id")]
	public Snowflake? ChannelId { get; set; }

	/// <summary>Timeout duration in seconds (TIMEOUT, max 2419200 — 4 weeks).</summary>
	[JsonPropertyName("duration_seconds")]
	public int? DurationSeconds { get; set; }

	/// <summary>Additional explanation shown to the member when their message is blocked (BLOCK_MESSAGE, max 150 chars).</summary>
	[JsonPropertyName("custom_message")]
	public string? CustomMessage { get; set; }
}

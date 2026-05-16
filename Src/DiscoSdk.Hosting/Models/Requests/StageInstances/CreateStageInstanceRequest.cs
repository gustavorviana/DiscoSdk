using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Models.Requests.StageInstances;

/// <summary>
/// Request body for <c>POST /stage-instances</c>.
/// Reference: https://discord.com/developers/docs/resources/stage-instance#create-stage-instance
/// </summary>
internal class CreateStageInstanceRequest
{
	/// <summary>The stage channel id to open the instance on.</summary>
	[JsonPropertyName("channel_id")]
	public string ChannelId { get; set; } = default!;

	/// <summary>Topic shown to listeners (1-120 chars).</summary>
	[JsonPropertyName("topic")]
	public string Topic { get; set; } = default!;

	/// <summary>Privacy level (defaults server-side to GuildOnly when omitted).</summary>
	[JsonPropertyName("privacy_level")]
	public StagePrivacyLevel? PrivacyLevel { get; set; }

	/// <summary>Whether to ping <c>@everyone</c> on start (requires permission).</summary>
	[JsonPropertyName("send_start_notification")]
	public bool? SendStartNotification { get; set; }

	/// <summary>Link this stage instance to an existing scheduled event.</summary>
	[JsonPropertyName("guild_scheduled_event_id")]
	public string? GuildScheduledEventId { get; set; }
}

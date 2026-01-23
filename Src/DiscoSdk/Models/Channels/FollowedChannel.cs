using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a followed channel object returned when following an announcement channel.
/// </summary>
public class FollowedChannel
{
	/// <summary>
	/// Gets or sets the ID of the target channel.
	/// </summary>
	[JsonPropertyName("channel_id")]
	public Snowflake ChannelId { get; set; }

	/// <summary>
	/// Gets or sets the ID of the webhook created for the target channel.
	/// </summary>
	[JsonPropertyName("webhook_id")]
	public Snowflake WebhookId { get; set; }
}


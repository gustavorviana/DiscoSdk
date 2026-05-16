using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Channels;

/// <inheritdoc cref="IFollowedChannel"/>
internal class FollowedChannel : IFollowedChannel
{
	[JsonPropertyName("channel_id")]
	public Snowflake ChannelId { get; init; }

	[JsonPropertyName("webhook_id")]
	public Snowflake WebhookId { get; init; }
}

using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <inheritdoc cref="IWelcomeScreenChannel"/>
internal class WelcomeScreenChannel : IWelcomeScreenChannel
{
	[JsonPropertyName("channel_id")]
	public Snowflake ChannelId { get; init; } = default!;

	[JsonPropertyName("description")]
	public string Description { get; init; } = default!;

	[JsonPropertyName("emoji_id")]
	public string? EmojiId { get; init; }

	[JsonPropertyName("emoji_name")]
	public string? EmojiName { get; init; }
}

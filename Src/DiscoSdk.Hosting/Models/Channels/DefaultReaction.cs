using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Channels;

/// <inheritdoc cref="IDefaultReaction"/>
internal class DefaultReaction : IDefaultReaction
{
	[JsonPropertyName("emoji_id")]
	public Snowflake? EmojiId { get; init; }

	[JsonPropertyName("emoji_name")]
	public string? EmojiName { get; init; }
}

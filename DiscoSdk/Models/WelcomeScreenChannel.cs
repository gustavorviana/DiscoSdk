using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a channel in a guild welcome screen.
/// </summary>
public class WelcomeScreenChannel
{
	/// <summary>
	/// Gets or sets the channel's unique identifier.
	/// </summary>
	[JsonPropertyName("channel_id")]
	public Snowflake ChannelId { get; set; } = default!;

	/// <summary>
	/// Gets or sets the description shown for the channel.
	/// </summary>
	[JsonPropertyName("description")]
	public string Description { get; set; } = default!;

	/// <summary>
	/// Gets or sets the emoji ID, if the emoji is custom.
	/// </summary>
	[JsonPropertyName("emoji_id")]
	public string? EmojiId { get; set; }

	/// <summary>
	/// Gets or sets the emoji name if custom, the unicode character if standard, or null if no emoji is set.
	/// </summary>
	[JsonPropertyName("emoji_name")]
	public string? EmojiName { get; set; }
}


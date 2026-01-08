using DiscoSdk.Models;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents the default reaction emoji for a forum channel.
/// </summary>
public class DefaultReaction
{
	/// <summary>
	/// Gets or sets the ID of a guild's custom emoji.
	/// </summary>
	[JsonPropertyName("emoji_id")]
	public DiscordId? EmojiId { get; set; }

	/// <summary>
	/// Gets or sets the unicode character of the emoji.
	/// </summary>
	[JsonPropertyName("emoji_name")]
	public string? EmojiName { get; set; }
}


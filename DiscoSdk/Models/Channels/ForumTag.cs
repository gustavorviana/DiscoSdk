using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a tag that can be applied to threads in a forum or media channel.
/// </summary>
public class ForumTag
{
	/// <summary>
	/// Gets or sets the ID of the tag.
	/// </summary>
	[JsonPropertyName("id")]
	public Snowflake? Id { get; set; }

	/// <summary>
	/// Gets or sets the name of the tag (max 20 characters).
	/// </summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = default!;

	/// <summary>
	/// Gets or sets whether this tag can only be applied to or removed from threads by members with the MANAGE_THREADS permission.
	/// </summary>
	[JsonPropertyName("moderated")]
	public bool Moderated { get; set; }

	/// <summary>
	/// Gets or sets the ID of a guild's custom emoji.
	/// </summary>
	[JsonPropertyName("emoji_id")]
	public Snowflake? EmojiId { get; set; }

	/// <summary>
	/// Gets or sets the unicode character of the emoji.
	/// </summary>
	[JsonPropertyName("emoji_name")]
	public string? EmojiName { get; set; }
}


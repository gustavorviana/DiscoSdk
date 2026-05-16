using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Channels;

/// <inheritdoc cref="IForumTag"/>
/// <remarks>Public because it doubles as the input shape for <see cref="Rest.Actions.ICreateChannelAction.SetAvailableTags"/>.</remarks>
public class ForumTag : IForumTag
{
	/// <inheritdoc />
	[JsonPropertyName("id")]
	public Snowflake? Id { get; init; }

	/// <inheritdoc />
	[JsonPropertyName("name")]
	public string Name { get; init; } = default!;

	/// <inheritdoc />
	[JsonPropertyName("moderated")]
	public bool Moderated { get; init; }

	/// <inheritdoc />
	[JsonPropertyName("emoji_id")]
	public Snowflake? EmojiId { get; init; }

	/// <inheritdoc />
	[JsonPropertyName("emoji_name")]
	public string? EmojiName { get; init; }
}

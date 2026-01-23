using DiscoSdk.Models.Messages;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a forum post (thread) created in a forum channel.
/// </summary>
public class ForumPost
{
	/// <summary>
	/// Gets or sets the message that was created as part of the forum post.
	/// </summary>
	[JsonPropertyName("message")]
	public Message Message { get; set; } = default!;

	/// <summary>
	/// Gets or sets the thread channel that was created for the forum post.
	/// </summary>
	[JsonPropertyName("channel")]
	public Channel Channel { get; set; } = default!;
}


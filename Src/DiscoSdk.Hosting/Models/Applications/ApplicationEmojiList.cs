using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Applications;

/// <summary>
/// Envelope returned by <c>GET /applications/{application.id}/emojis</c>. Discord wraps the emoji
/// list in an <c>items</c> property; the SDK unwraps before exposing the public surface.
/// Internal — only seen by <see cref="Hosting.Rest.Clients.ApplicationClient"/>.
/// </summary>
internal class ApplicationEmojiList
{
	/// <summary>The emoji objects.</summary>
	[JsonPropertyName("items")]
	public InternalEmoji[] Items { get; set; } = [];
}

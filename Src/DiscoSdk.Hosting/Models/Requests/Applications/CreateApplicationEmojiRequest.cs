using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Models.Requests.Applications;

/// <summary>
/// Request body for <c>POST /applications/{application.id}/emojis</c>.
/// Reference: https://discord.com/developers/docs/resources/emoji#create-application-emoji
/// </summary>
internal class CreateApplicationEmojiRequest
{
	/// <summary>Emoji name (2-32 chars).</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = default!;

	/// <summary>The 128x128 emoji image as a data URI (e.g. <c>data:image/png;base64,...</c>).</summary>
	[JsonPropertyName("image")]
	public string Image { get; set; } = default!;
}

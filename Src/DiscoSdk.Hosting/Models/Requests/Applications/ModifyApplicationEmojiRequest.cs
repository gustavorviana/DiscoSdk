using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Models.Requests.Applications;

/// <summary>
/// Request body for <c>PATCH /applications/{application.id}/emojis/{emoji.id}</c>. Only the
/// emoji's name can be changed — Discord doesn't accept image edits on existing application
/// emojis.
/// Reference: https://discord.com/developers/docs/resources/emoji#modify-application-emoji
/// </summary>
internal class ModifyApplicationEmojiRequest
{
	/// <summary>New emoji name (2-32 chars).</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = default!;
}

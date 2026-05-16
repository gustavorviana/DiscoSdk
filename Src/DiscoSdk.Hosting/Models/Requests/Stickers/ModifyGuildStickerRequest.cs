using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Models.Requests.Stickers;

/// <summary>
/// Request body for <c>PATCH /guilds/{guild.id}/stickers/{sticker.id}</c>.
/// Reference: https://discord.com/developers/docs/resources/sticker#modify-guild-sticker
/// </summary>
internal class ModifyGuildStickerRequest
{
	/// <summary>New sticker name (2-30 chars).</summary>
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	/// <summary>New description (2-100 chars or null to clear).</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>New autocomplete/suggestion tags (max 200 chars).</summary>
	[JsonPropertyName("tags")]
	public string? Tags { get; set; }
}

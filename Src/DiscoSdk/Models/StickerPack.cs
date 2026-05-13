using DiscoSdk.Models.Messages;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// A Discord-curated sticker pack. Returned by <c>GET /sticker-packs</c> (Nitro defaults).
/// Internal — consumers use <see cref="IStickerPack"/>.
/// </summary>
internal class StickerPack
{
	/// <summary>ID of the sticker pack.</summary>
	[JsonPropertyName("id")]
	public Snowflake Id { get; set; } = default!;

	/// <summary>The stickers in the pack.</summary>
	[JsonPropertyName("stickers")]
	public Sticker[] Stickers { get; set; } = [];

	/// <summary>Name of the sticker pack.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>ID of the SKU representing the pack.</summary>
	[JsonPropertyName("sku_id")]
	public Snowflake SkuId { get; set; } = default!;

	/// <summary>ID of a sticker in the pack used as the pack's icon.</summary>
	[JsonPropertyName("cover_sticker_id")]
	public Snowflake? CoverStickerId { get; set; }

	/// <summary>Description of the sticker pack.</summary>
	[JsonPropertyName("description")]
	public string Description { get; set; } = string.Empty;

	/// <summary>Asset id of the pack's banner image.</summary>
	[JsonPropertyName("banner_asset_id")]
	public Snowflake? BannerAssetId { get; set; }
}

/// <summary>
/// Envelope returned by <c>GET /sticker-packs</c> — Discord wraps the pack array in an object.
/// Internal — only seen by <see cref="Hosting.Rest.Clients.StickerClient"/>.
/// </summary>
internal class StickerPackList
{
	/// <summary>The sticker packs.</summary>
	[JsonPropertyName("sticker_packs")]
	public StickerPack[] StickerPacks { get; set; } = [];
}

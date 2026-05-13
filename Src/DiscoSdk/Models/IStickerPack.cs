using System.Collections.Immutable;

namespace DiscoSdk.Models;

/// <summary>
/// A Discord-curated sticker pack (Nitro defaults). Read-only on the public surface — packs
/// can't be modified or deleted by bots.
/// </summary>
public interface IStickerPack : IWithSnowflake
{
	/// <summary>Pack name.</summary>
	string Name { get; }

	/// <summary>Pack description.</summary>
	string Description { get; }

	/// <summary>The stickers in the pack.</summary>
	ImmutableArray<ISticker> Stickers { get; }

	/// <summary>SKU representing the pack.</summary>
	Snowflake SkuId { get; }

	/// <summary>If set, the sticker used as the pack's cover/icon.</summary>
	Snowflake? CoverStickerId { get; }

	/// <summary>Asset id for the pack's banner.</summary>
	Snowflake? BannerAssetId { get; }
}

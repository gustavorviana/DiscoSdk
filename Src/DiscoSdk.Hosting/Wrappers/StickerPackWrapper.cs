using DiscoSdk.Models;
using System.Collections.Immutable;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>Read-only public wrapper over a built-in <see cref="StickerPack"/>.</summary>
internal sealed class StickerPackWrapper(DiscordClient client, StickerPack model) : IStickerPack
{
	/// <inheritdoc />
	public Snowflake Id => model.Id;
	/// <inheritdoc />
	public DateTimeOffset CreatedAt => model.Id.CreatedAt;
	/// <inheritdoc />
	public string Name => model.Name;
	/// <inheritdoc />
	public string Description => model.Description;
	/// <inheritdoc />
	public ImmutableArray<ISticker> Stickers =>
		[.. model.Stickers.Select(s => (ISticker)new StickerWrapper(client, s))];
	/// <inheritdoc />
	public Snowflake SkuId => model.SkuId;
	/// <inheritdoc />
	public Snowflake? CoverStickerId => model.CoverStickerId;
	/// <inheritdoc />
	public Snowflake? BannerAssetId => model.BannerAssetId;
}

using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Fluent builder for <c>PATCH /guilds/{guild.id}/stickers/{sticker.id}</c>. Only fields you
/// touch are sent on the wire.
/// </summary>
public interface IModifyGuildStickerAction : IRestAction<ISticker>
{
	/// <summary>Renames the sticker (2-30 chars).</summary>
	IModifyGuildStickerAction SetName(string name);

	/// <summary>Updates the description (2-100 chars).</summary>
	IModifyGuildStickerAction SetDescription(string description);

	/// <summary>Updates the suggestion tag string (max 200 chars).</summary>
	IModifyGuildStickerAction SetTags(string tags);
}

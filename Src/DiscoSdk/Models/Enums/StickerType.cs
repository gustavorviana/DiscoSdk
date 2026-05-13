namespace DiscoSdk.Models.Enums;

/// <summary>
/// Whether a sticker belongs to one of Discord's built-in packs or to a guild.
/// </summary>
public enum StickerType
{
	/// <summary>Sticker that belongs to one of Discord's default Nitro sticker packs.</summary>
	Standard = 1,

	/// <summary>Sticker that was uploaded by a guild as part of the guild's custom sticker set.</summary>
	Guild = 2,
}

using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models;

/// <summary>
/// Public surface for a Discord sticker — either a built-in pack sticker (<see cref="StickerType.Standard"/>)
/// or a guild-uploaded one (<see cref="StickerType.Guild"/>). Modify / Delete actions only succeed
/// on guild stickers.
/// </summary>
public interface ISticker : IWithSnowflake
{
	/// <summary>Sticker name (2-30 chars).</summary>
	string Name { get; }

	/// <summary>Sticker description (2-100 chars).</summary>
	string? Description { get; }

	/// <summary>Autocomplete/suggestion tag string (max 200 chars).</summary>
	string? Tags { get; }

	/// <summary>Whether this is a standard (Nitro) or guild-uploaded sticker.</summary>
	StickerType Type { get; }

	/// <summary>Encoding format (PNG / APNG / Lottie / GIF).</summary>
	StickerFormatType FormatType { get; }

	/// <summary>For guild stickers, whether the sticker is usable (becomes false on lost Nitro tier).</summary>
	bool? Available { get; }

	/// <summary>For guild stickers, the guild that owns it.</summary>
	Snowflake? GuildId { get; }

	/// <summary>For standard stickers, the pack they belong to.</summary>
	Snowflake? PackId { get; }

	/// <summary>For standard stickers, the sort order within the pack.</summary>
	int? SortValue { get; }

	/// <summary>For guild stickers, the user who uploaded it (requires MANAGE_GUILD_EXPRESSIONS to populate).</summary>
	IUser? User { get; }

	/// <summary>
	/// Modifies a guild sticker (name / description / tags). Returns a fluent builder.
	/// Standard pack stickers cannot be modified — Discord rejects the call.
	/// </summary>
	IModifyGuildStickerAction Modify();

	/// <summary>
	/// Deletes a guild sticker. Standard pack stickers cannot be deleted.
	/// </summary>
	IRestAction Delete();
}

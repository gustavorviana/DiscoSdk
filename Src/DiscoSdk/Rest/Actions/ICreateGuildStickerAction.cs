using DiscoSdk.Models;
using DiscoSdk.Models.Messages;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Fluent builder for <c>POST /guilds/{guild.id}/stickers</c>. Multipart upload — the factory
/// supplies the file; required fields <c>name</c>, <c>tags</c> and the file are required,
/// while <c>description</c> is optional.
/// </summary>
public interface ICreateGuildStickerAction : IRestAction<ISticker>
{
	/// <summary>Sets the description (2-100 chars).</summary>
	ICreateGuildStickerAction SetDescription(string description);

	/// <summary>Overrides the tags supplied at construction time.</summary>
	ICreateGuildStickerAction SetTags(string tags);

	/// <summary>Replaces the file payload (PNG/APNG/GIF/LOTTIE, max 512 KiB).</summary>
	ICreateGuildStickerAction SetFile(MessageFile file);
}

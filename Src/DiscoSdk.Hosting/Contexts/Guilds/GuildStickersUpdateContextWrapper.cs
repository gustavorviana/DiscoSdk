using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Messages;
using System.Collections.Immutable;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class GuildStickersUpdateContextWrapper(DiscordClient client, IGuild guild, ImmutableArray<Sticker> stickers)
	: ContextWrapper(client), IGuildStickersUpdateContext
{
	private ImmutableArray<ISticker>? _wrapped;
	public IGuild Guild => guild;
	public ImmutableArray<ISticker> Stickers => _wrapped ??= [.. stickers.Select(s => new StickerWrapper(client, s))];
}

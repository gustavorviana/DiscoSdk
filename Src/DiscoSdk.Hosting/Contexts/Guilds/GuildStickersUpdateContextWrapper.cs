using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models;
using DiscoSdk.Models.Messages;
using System.Collections.Immutable;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class GuildStickersUpdateContextWrapper(DiscordClient client, IGuild guild, ImmutableArray<Sticker> stickers)
	: ContextWrapper(client), IGuildStickersUpdateContext
{
	public IGuild Guild => guild;
	public ImmutableArray<Sticker> Stickers => stickers;
}

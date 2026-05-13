using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models;
using System.Collections.Immutable;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class GuildEmojisUpdateContextWrapper(DiscordClient client, IGuild guild, ImmutableArray<Emoji> emojis)
	: ContextWrapper(client), IGuildEmojisUpdateContext
{
	public IGuild Guild => guild;
	public ImmutableArray<Emoji> Emojis => emojis;
}

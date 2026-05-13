using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class GuildBanContextWrapper(DiscordClient client, IUser user, IGuild guild)
	: ContextWrapper(client), IGuildBanContext
{
	public IUser User => user;
	public IGuild Guild => guild;
}

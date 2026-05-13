using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class GuildMemberRemoveContextWrapper(DiscordClient client, IUser user, IGuild guild)
	: ContextWrapper(client), IGuildMemberRemoveContext
{
	public IUser User => user;
	public IGuild Guild => guild;
}

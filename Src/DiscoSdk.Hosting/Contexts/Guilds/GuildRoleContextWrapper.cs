using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class GuildRoleContextWrapper(DiscordClient client, IRole role, IGuild guild)
	: ContextWrapper(client), IGuildRoleContext
{
	public IRole Role => role;
	public IGuild Guild => guild;
}

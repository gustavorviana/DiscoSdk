using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class GuildRoleDeleteContextWrapper(DiscordClient client, Snowflake roleId, IGuild guild)
	: ContextWrapper(client), IGuildRoleDeleteContext
{
	public Snowflake RoleId => roleId;
	public IGuild Guild => guild;
}

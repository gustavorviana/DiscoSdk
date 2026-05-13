using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class GuildIntegrationsUpdateContextWrapper(DiscordClient client, IGuild guild)
	: ContextWrapper(client), IGuildIntegrationsUpdateContext
{
	public IGuild Guild => guild;
}

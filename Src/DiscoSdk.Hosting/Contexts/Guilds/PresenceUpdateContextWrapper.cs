using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models;
using DiscoSdk.Models.Presences;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class PresenceUpdateContextWrapper(DiscordClient client, Presence presence, IGuild? guild)
	: ContextWrapper(client), IPresenceUpdateContext
{
	public Presence Presence => presence;
	public IGuild? Guild => guild;
}

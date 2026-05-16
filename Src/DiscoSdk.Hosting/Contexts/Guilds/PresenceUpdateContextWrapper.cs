using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Presences;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class PresenceUpdateContextWrapper(DiscordClient client, Presence presence, IGuild? guild)
	: ContextWrapper(client), IPresenceUpdateContext
{
	private IPresence? _wrapped;
	public IPresence Presence => _wrapped ??= new PresenceWrapper(presence);
	public IGuild? Guild => guild;
}

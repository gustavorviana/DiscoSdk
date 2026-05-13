using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class InviteDeleteContextWrapper(DiscordClient client,
	string code,
	ITextBasedChannel channel,
	IGuild? guild) : ContextWrapper(client), IInviteDeleteContext
{
	public string Code => code;
	public ITextBasedChannel Channel => channel;
	public IGuild? Guild => guild;
}

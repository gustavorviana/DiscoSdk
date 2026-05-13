using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class GuildMemberUpdateContextWrapper(DiscordClient client, IMember member, IGuild guild)
	: ContextWrapper(client), IGuildMemberUpdateContext
{
	public IMember Member => member;
	public IGuild Guild => guild;
}

using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class GuildMemberAddContextWrapper(DiscordClient client, IMember member, IGuild guild)
	: ContextWrapper(client), IGuildMemberAddContext
{
	public IMember Member => member;
	public IGuild Guild => guild;
}

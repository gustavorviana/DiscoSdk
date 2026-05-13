using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class InviteCreateContextWrapper(DiscordClient client,
	string code,
	ITextBasedChannel channel,
	IGuild? guild,
	IUser? inviter,
	DateTimeOffset createdAt,
	int maxAge,
	int maxUses,
	bool temporary) : ContextWrapper(client), IInviteCreateContext
{
	public string Code => code;
	public ITextBasedChannel Channel => channel;
	public IGuild? Guild => guild;
	public IUser? Inviter => inviter;
	public DateTimeOffset CreatedAt => createdAt;
	public int MaxAge => maxAge;
	public int MaxUses => maxUses;
	public bool Temporary => temporary;
}

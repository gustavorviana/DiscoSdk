using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class GuildScheduledEventContextWrapper(DiscordClient client, GuildScheduledEvent scheduledEvent, IGuild guild)
	: ContextWrapper(client), IGuildScheduledEventContext
{
	public GuildScheduledEvent ScheduledEvent => scheduledEvent;
	public IGuild Guild => guild;
}

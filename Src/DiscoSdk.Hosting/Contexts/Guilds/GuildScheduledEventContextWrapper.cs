using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class GuildScheduledEventContextWrapper(DiscordClient client, IGuildScheduledEvent scheduledEvent, IGuild guild)
	: ContextWrapper(client), IGuildScheduledEventContext
{
	public IGuildScheduledEvent ScheduledEvent => scheduledEvent;
	public IGuild Guild => guild;
}

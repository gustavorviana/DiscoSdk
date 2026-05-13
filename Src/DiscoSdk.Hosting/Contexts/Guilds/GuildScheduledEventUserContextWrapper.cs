using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class GuildScheduledEventUserContextWrapper(DiscordClient client,
	Snowflake scheduledEventId,
	Snowflake userId,
	IGuild guild) : ContextWrapper(client), IGuildScheduledEventUserContext
{
	public Snowflake ScheduledEventId => scheduledEventId;
	public Snowflake UserId => userId;
	public IGuild Guild => guild;
}

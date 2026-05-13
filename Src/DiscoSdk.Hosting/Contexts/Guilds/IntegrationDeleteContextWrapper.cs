using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class IntegrationDeleteContextWrapper(DiscordClient client,
	Snowflake integrationId,
	IGuild guild,
	Snowflake? applicationId) : ContextWrapper(client), IIntegrationDeleteContext
{
	public Snowflake IntegrationId => integrationId;
	public IGuild Guild => guild;
	public Snowflake? ApplicationId => applicationId;
}

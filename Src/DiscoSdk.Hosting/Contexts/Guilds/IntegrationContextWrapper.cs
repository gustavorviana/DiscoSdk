using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class IntegrationContextWrapper(DiscordClient client, IIntegration integration, IGuild guild)
	: ContextWrapper(client), IIntegrationContext
{
	public IIntegration Integration => integration;
	public IGuild Guild => guild;
}

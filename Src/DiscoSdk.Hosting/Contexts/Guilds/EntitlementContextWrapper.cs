using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models.Monetization;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class EntitlementContextWrapper(DiscordClient client, Entitlement entitlement)
	: ContextWrapper(client), IEntitlementContext
{
	public Entitlement Entitlement => entitlement;
}

using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models.Monetization;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class EntitlementContextWrapper(DiscordClient client, Entitlement entitlement)
	: ContextWrapper(client), IEntitlementContext
{
	private IEntitlement? _wrapped;
	public IEntitlement Entitlement => _wrapped ??= new EntitlementWrapper(client, entitlement);
}

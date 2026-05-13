using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models.Monetization;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class SubscriptionContextWrapper(DiscordClient client, ISubscription subscription)
	: ContextWrapper(client), ISubscriptionContext
{
	public ISubscription Subscription => subscription;
}

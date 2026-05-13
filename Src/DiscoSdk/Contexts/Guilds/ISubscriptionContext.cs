using DiscoSdk.Models.Monetization;

namespace DiscoSdk.Contexts.Guilds;

/// <summary>
/// Context for <c>SUBSCRIPTION_CREATE</c>, <c>SUBSCRIPTION_UPDATE</c>, and <c>SUBSCRIPTION_DELETE</c>
/// Gateway events — a user's premium subscription state changed for the application.
/// </summary>
public interface ISubscriptionContext : IContext
{
	/// <summary>The subscription payload.</summary>
	ISubscription Subscription { get; }
}

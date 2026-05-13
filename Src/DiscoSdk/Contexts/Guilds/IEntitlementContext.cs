using DiscoSdk.Models.Monetization;

namespace DiscoSdk.Contexts.Guilds;

/// <summary>
/// Context for <c>ENTITLEMENT_CREATE</c>, <c>ENTITLEMENT_UPDATE</c>, and <c>ENTITLEMENT_DELETE</c>
/// Gateway events.
/// </summary>
public interface IEntitlementContext : IContext
{
	/// <summary>The entitlement payload.</summary>
	Entitlement Entitlement { get; }
}

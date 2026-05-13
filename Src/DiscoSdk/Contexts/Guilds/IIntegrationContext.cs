using DiscoSdk.Models;

namespace DiscoSdk.Contexts.Guilds;

/// <summary>
/// Context for <c>INTEGRATION_CREATE</c> and <c>INTEGRATION_UPDATE</c> Gateway events.
/// </summary>
public interface IIntegrationContext : IContext
{
	/// <summary>The integration payload.</summary>
	IIntegration Integration { get; }

	/// <summary>The guild containing the integration.</summary>
	IGuild Guild { get; }
}

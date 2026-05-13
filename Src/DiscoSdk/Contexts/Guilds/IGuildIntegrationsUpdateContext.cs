using DiscoSdk.Models;

namespace DiscoSdk.Contexts.Guilds;

/// <summary>
/// Context for the <c>GUILD_INTEGRATIONS_UPDATE</c> Gateway event — a hint that a guild's integrations
/// changed; the payload only carries the guild ID, so handlers typically refetch via REST.
/// </summary>
public interface IGuildIntegrationsUpdateContext : IContext
{
	/// <summary>The guild whose integrations changed.</summary>
	IGuild Guild { get; }
}

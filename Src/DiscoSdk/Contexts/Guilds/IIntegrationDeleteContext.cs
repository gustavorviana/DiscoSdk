using DiscoSdk.Models;

namespace DiscoSdk.Contexts.Guilds;

/// <summary>
/// Context for the <c>INTEGRATION_DELETE</c> Gateway event — the integration object is gone, only
/// identifiers remain.
/// </summary>
public interface IIntegrationDeleteContext : IContext
{
	/// <summary>ID of the deleted integration.</summary>
	Snowflake IntegrationId { get; }

	/// <summary>The guild the integration belonged to.</summary>
	IGuild Guild { get; }

	/// <summary>ID of the bot/OAuth2 application this integration was tied to, if any.</summary>
	Snowflake? ApplicationId { get; }
}

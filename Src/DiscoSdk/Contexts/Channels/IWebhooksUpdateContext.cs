using DiscoSdk.Models;

namespace DiscoSdk.Contexts.Channels;

/// <summary>
/// Context for the <c>WEBHOOKS_UPDATE</c> Gateway event — a webhook was created, updated, or deleted
/// for a given guild channel. The payload only carries identifiers; resolve the channel via
/// <see cref="IDiscordClient.GetChannel"/> if needed.
/// </summary>
public interface IWebhooksUpdateContext : IContext
{
	/// <summary>The guild containing the channel.</summary>
	IGuild Guild { get; }

	/// <summary>The ID of the channel whose webhooks changed.</summary>
	Snowflake ChannelId { get; }
}

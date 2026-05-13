using DiscoSdk.Contexts.Channels;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Contexts.Channels;

internal class WebhooksUpdateContextWrapper(DiscordClient client, IGuild guild, Snowflake channelId)
	: ContextWrapper(client), IWebhooksUpdateContext
{
	public IGuild Guild => guild;
	public Snowflake ChannelId => channelId;
}

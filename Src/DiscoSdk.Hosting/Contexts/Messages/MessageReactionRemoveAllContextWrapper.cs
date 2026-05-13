using DiscoSdk.Contexts.Messages;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;

namespace DiscoSdk.Hosting.Contexts.Messages;

internal class MessageReactionRemoveAllContextWrapper(DiscordClient client,
	Snowflake messageId,
	ITextBasedChannel channel,
	IGuild? guild) : ContextWrapper(client), IMessageReactionRemoveAllContext
{
	public Snowflake MessageId => messageId;
	public ITextBasedChannel Channel => channel;
	public IGuild? Guild => guild;
}

using DiscoSdk.Contexts.Messages;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;

namespace DiscoSdk.Hosting.Contexts.Messages;

internal class MessagePollVoteContextWrapper(DiscordClient client,
	Snowflake userId,
	Snowflake messageId,
	int answerId,
	ITextBasedChannel channel,
	IGuild? guild) : ContextWrapper(client), IMessagePollVoteContext
{
	public Snowflake UserId => userId;
	public Snowflake MessageId => messageId;
	public int AnswerId => answerId;
	public ITextBasedChannel Channel => channel;
	public IGuild? Guild => guild;
}

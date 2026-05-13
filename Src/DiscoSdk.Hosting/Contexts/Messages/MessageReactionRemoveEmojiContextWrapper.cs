using DiscoSdk.Contexts.Messages;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;

namespace DiscoSdk.Hosting.Contexts.Messages;

internal class MessageReactionRemoveEmojiContextWrapper(DiscordClient client,
	Snowflake messageId,
	IEmoji emoji,
	ITextBasedChannel channel,
	IGuild? guild) : ContextWrapper(client), IMessageReactionRemoveEmojiContext
{
	public Snowflake MessageId => messageId;
	public IEmoji Emoji => emoji;
	public ITextBasedChannel Channel => channel;
	public IGuild? Guild => guild;
}

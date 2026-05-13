using DiscoSdk.Contexts.Messages;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using System.Collections.Immutable;

namespace DiscoSdk.Hosting.Contexts.Messages;

internal class MessageDeleteBulkContextWrapper(DiscordClient client,
	ImmutableArray<Snowflake> ids,
	ITextBasedChannel channel,
	IGuild? guild) : ContextWrapper(client), IMessageDeleteBulkContext
{
	public ImmutableArray<Snowflake> Ids => ids;
	public ITextBasedChannel Channel => channel;
	public IGuild? Guild => guild;
}

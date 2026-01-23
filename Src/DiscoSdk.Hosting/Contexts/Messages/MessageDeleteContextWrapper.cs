using DiscoSdk.Contexts.Messages;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;

namespace DiscoSdk.Hosting.Contexts.Messages;

internal class MessageDeleteContextWrapper(DiscordClient client, 
    Snowflake id, 
    ITextBasedChannel channel,
    IGuild? guild) : ContextWrapper(client), IMessageDeleteContext
{
    public Snowflake Id => id;

    public ITextBasedChannel Channel => channel;

    public IGuild? Guild => guild;
}

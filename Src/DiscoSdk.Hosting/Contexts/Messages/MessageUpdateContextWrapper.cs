using DiscoSdk.Contexts.Channels;
using DiscoSdk.Contexts.Messages;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Messages;

namespace DiscoSdk.Hosting.Contexts.Messages;

internal class MessageUpdateContextWrapper(DiscordClient client,
    IGuild? guild,
    IUser author,
    IMember? member,
    IMessage message,
    ITextBasedChannel channel) : MemberContextWrapper(client, guild, author, member), IMessageUpdateContext
{
    public IMessage Message => message;

    public ITextBasedChannel Channel => channel;

    IChannel IChannelContextBase.Channel => Channel;
}

using DiscoSdk.Contexts.Channels;
using DiscoSdk.Contexts.Messages;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Messages;

namespace DiscoSdk.Hosting.Contexts.Wrappers;

internal class MessageCreateContextWrapper(DiscordClient client,
    IMember? member,
    IMessage message,
    ITextBasedChannel channel)
    : ContextWrapper(client), IMessageCreateContext
{
    public IMessage Message => message;

    public ITextBasedChannel Channel => channel;

    public IGuild? Guild => member?.Guild;

    public IMember? Member => member;

    public IUser Author => message.Author;

    IChannel IChannelContextBase.Channel => Channel;
}
using DiscoSdk.Contexts.Channels;
using DiscoSdk.Models.Messages;

namespace DiscoSdk.Contexts.Messages;

public interface IMessageContext : IWithTextChannel
{
    IMessage Message { get; }
}

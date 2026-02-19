using DiscoSdk.Models.Channels;

namespace DiscoSdk.Contexts.Channels;

public interface IChannelContextBase<TChannel> : IChannelContextBase where TChannel : IChannel
{
    public new TChannel Channel { get; }
}

public interface IChannelContextBase : IContext
{
    IChannel Channel { get; }
}

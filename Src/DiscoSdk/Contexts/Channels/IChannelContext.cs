using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models.Channels;

namespace DiscoSdk.Contexts.Channels;

/// <summary>
/// Represents the event data for when a channel is created.
/// </summary>
public interface IChannelContext : IGuildContext, IChannelContextBase<IGuildChannelUnion>;
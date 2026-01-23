using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models.Channels;

namespace DiscoSdk.Contexts.Channels;

/// <summary>
/// Represents the event data for when a channel is created.
/// </summary>
public interface IChannelContext : IGuildContext
{
    /// <summary>
    /// Gets or sets the channel that was created.
    /// </summary>
    IGuildChannelUnion Channel { get; }
}
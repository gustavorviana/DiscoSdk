using DiscoSdk.Models.Enums;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents the base interface for all Discord channels.
/// </summary>
public interface IChannel : IMentionable, IDeletable
{
    string Name { get; }

    /// <summary>
    /// Gets the type of the channel.
    /// </summary>
    ChannelType Type { get; }
}
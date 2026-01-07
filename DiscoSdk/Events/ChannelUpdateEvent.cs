using DiscoSdk.Models.Channels;

namespace DiscoSdk.Events;

/// <summary>
/// Represents the event data for when a channel is updated.
/// </summary>
public class ChannelUpdateEvent
{
    /// <summary>
    /// Gets or sets the updated channel data.
    /// </summary>
    public Channel Channel { get; set; } = default!;
}


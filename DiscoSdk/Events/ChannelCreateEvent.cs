using DiscoSdk.Models;

namespace DiscoSdk.Events;

/// <summary>
/// Represents the event data for when a channel is created.
/// </summary>
public class ChannelCreateEvent
{
    /// <summary>
    /// Gets or sets the channel that was created.
    /// </summary>
    public Channel Channel { get; set; } = default!;
}


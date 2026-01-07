using DiscoSdk.Models.Channels;

namespace DiscoSdk.Events;

/// <summary>
/// Represents the event data for when a channel is deleted.
/// </summary>
public class ChannelDeleteEvent
{
    /// <summary>
    /// Gets or sets the channel that was deleted.
    /// </summary>
    public Channel Channel { get; set; } = default!;
}


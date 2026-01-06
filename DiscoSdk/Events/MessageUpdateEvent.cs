using DiscoSdk.Models;

namespace DiscoSdk.Events;

/// <summary>
/// Represents the event data for when a message is updated.
/// </summary>
public class MessageUpdateEvent
{
    /// <summary>
    /// Gets or sets the updated message data.
    /// </summary>
    public Message Message { get; set; } = default!;
}


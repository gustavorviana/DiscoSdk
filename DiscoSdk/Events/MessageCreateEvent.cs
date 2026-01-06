using DiscoSdk.Models;

namespace DiscoSdk.Events;

/// <summary>
/// Represents the event data for when a message is created.
/// </summary>
public class MessageCreateEvent
{
    /// <summary>
    /// Gets or sets the message that was created.
    /// </summary>
    public Message Message { get; set; } = default!;
}


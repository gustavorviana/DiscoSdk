namespace DiscoSdk.Events;

/// <summary>
/// Represents the event data for when a message is deleted.
/// </summary>
public class MessageDeleteEvent
{
    /// <summary>
    /// Gets or sets the ID of the deleted message.
    /// </summary>
    public string Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the ID of the channel where the message was deleted.
    /// </summary>
    public string ChannelId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the ID of the guild where the message was deleted.
    /// </summary>
    public string? GuildId { get; set; }
}


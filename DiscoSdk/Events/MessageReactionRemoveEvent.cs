using DiscoSdk.Models;

namespace DiscoSdk.Events;

/// <summary>
/// Represents the event data for when a reaction is removed from a message.
/// </summary>
public class MessageReactionRemoveEvent
{
    /// <summary>
    /// Gets or sets the ID of the user who removed the reaction.
    /// </summary>
    public string UserId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the ID of the channel where the reaction was removed.
    /// </summary>
    public string ChannelId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the ID of the message that had the reaction removed.
    /// </summary>
    public string MessageId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the ID of the guild where the reaction was removed.
    /// </summary>
    public string? GuildId { get; set; }

    /// <summary>
    /// Gets or sets the emoji that was removed.
    /// </summary>
    public Emoji Emoji { get; set; } = default!;
}


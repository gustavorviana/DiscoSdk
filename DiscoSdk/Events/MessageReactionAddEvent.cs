using DiscoSdk.Models;

namespace DiscoSdk.Events;

/// <summary>
/// Represents the event data for when a reaction is added to a message.
/// </summary>
public class MessageReactionAddEvent
{
    /// <summary>
    /// Gets or sets the ID of the user who added the reaction.
    /// </summary>
    public string UserId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the ID of the channel where the reaction was added.
    /// </summary>
    public string ChannelId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the ID of the message that was reacted to.
    /// </summary>
    public string MessageId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the ID of the guild where the reaction was added.
    /// </summary>
    public string? GuildId { get; set; }

    /// <summary>
    /// Gets or sets the member who added the reaction, if it happened in a guild.
    /// </summary>
    public GuildMember? Member { get; set; }

    /// <summary>
    /// Gets or sets the emoji that was used for the reaction.
    /// </summary>
    public Emoji Emoji { get; set; } = default!;
}


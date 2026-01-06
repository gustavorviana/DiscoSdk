using DiscoSdk.Models;

namespace DiscoSdk.Events;

/// <summary>
/// Represents the event data for when a user starts typing.
/// </summary>
public class TypingStartEvent
{
    /// <summary>
    /// Gets or sets the ID of the channel where the user started typing.
    /// </summary>
    public string ChannelId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the ID of the guild where the user started typing.
    /// </summary>
    public string? GuildId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who started typing.
    /// </summary>
    public string UserId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the unix time (in seconds) of when the user started typing.
    /// </summary>
    public int Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the member who started typing, if it happened in a guild.
    /// </summary>
    public GuildMember? Member { get; set; }
}


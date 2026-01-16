using DiscoSdk.Models;

namespace DiscoSdk.Events;

/// <summary>
/// Represents the event data for when a guild is deleted or becomes unavailable.
/// </summary>
public class GuildDeleteEvent
{
    /// <summary>
    /// Gets or sets the ID of the guild that was deleted or became unavailable.
    /// </summary>
    public DiscordId Id { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the guild is unavailable.
    /// </summary>
    public bool Unavailable { get; set; }
}


using DiscoSdk.Models;

namespace DiscoSdk.Events;

/// <summary>
/// Represents the event data for when a guild is created or becomes available.
/// </summary>
public class GuildCreateEvent
{
    /// <summary>
    /// Gets or sets the guild that was created or became available.
    /// </summary>
    public Guild Guild { get; set; } = default!;
}


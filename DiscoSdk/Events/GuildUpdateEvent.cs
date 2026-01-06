using DiscoSdk.Models;

namespace DiscoSdk.Events;

/// <summary>
/// Represents the event data for when a guild is updated.
/// </summary>
public class GuildUpdateEvent
{
    /// <summary>
    /// Gets or sets the updated guild data.
    /// </summary>
    public Guild Guild { get; set; } = default!;
}


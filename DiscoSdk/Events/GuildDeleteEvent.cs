namespace DiscoSdk.Events;

/// <summary>
/// Represents the event data for when a guild is deleted or becomes unavailable.
/// </summary>
public class GuildDeleteEvent
{
    /// <summary>
    /// Gets or sets the ID of the guild that was deleted or became unavailable.
    /// </summary>
    public string Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets a value indicating whether the guild is unavailable.
    /// </summary>
    public bool Unavailable { get; set; }
}


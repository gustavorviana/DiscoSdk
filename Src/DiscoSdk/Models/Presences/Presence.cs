using DiscoSdk.Models.Activities;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Presences;

/// <summary>
/// Represents a presence update of a user in a Discord guild.
/// </summary>
public class Presence
{
    /// <summary>
    /// Gets or sets the user information.
    /// </summary>
    [JsonPropertyName("user")]
    public PresenceUser? User { get; set; }

    /// <summary>
    /// Gets or sets the status of the user.
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the presence was processed.
    /// </summary>
    [JsonPropertyName("processed_at_timestamp")]
    public long ProcessedAtTimestamp { get; set; }

    /// <summary>
    /// Gets or sets the game activity.
    /// </summary>
    [JsonPropertyName("game")]
    public Activity? Game { get; set; }

    /// <summary>
    /// Gets or sets the client status.
    /// </summary>
    [JsonPropertyName("client_status")]
    public ClientStatus? ClientStatus { get; set; }

    /// <summary>
    /// Gets or sets the activities of the user.
    /// </summary>
    [JsonPropertyName("activities")]
    public Activity[] Activities { get; set; } =[];
}

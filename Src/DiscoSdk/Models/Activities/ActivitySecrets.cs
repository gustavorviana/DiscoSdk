using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Activities;

/// <summary>
/// Represents secrets for joining and spectating an activity.
/// </summary>
public class ActivitySecrets
{
    /// <summary>
    /// Gets or sets the secret for joining a party.
    /// </summary>
    [JsonPropertyName("join")]
    public string? Join { get; set; }

    /// <summary>
    /// Gets or sets the secret for spectating.
    /// </summary>
    [JsonPropertyName("spectate")]
    public string? Spectate { get; set; }

    /// <summary>
    /// Gets or sets the secret for a specific instanced match.
    /// </summary>
    [JsonPropertyName("match")]
    public string? Match { get; set; }
}
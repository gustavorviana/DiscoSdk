using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Activities;

/// <summary>
/// Represents party information for an activity.
/// </summary>
public class ActivityParty
{
    /// <summary>
    /// Gets or sets the party ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the party size.
    /// </summary>
    [JsonPropertyName("size")]
    public int[]? Size { get; set; }
}
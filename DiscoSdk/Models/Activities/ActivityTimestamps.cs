using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Activities;

/// <summary>
/// Represents timestamps for an activity.
/// </summary>
public class ActivityTimestamps
{
    /// <summary>
    /// Gets or sets the start timestamp.
    /// </summary>
    [JsonPropertyName("start")]
    public long? Start { get; set; }

    /// <summary>
    /// Gets or sets the end timestamp.
    /// </summary>
    [JsonPropertyName("end")]
    public long? End { get; set; }
}

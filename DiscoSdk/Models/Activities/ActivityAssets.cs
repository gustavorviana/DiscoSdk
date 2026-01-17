using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Activities;

/// <summary>
/// Represents assets for an activity.
/// </summary>
public class ActivityAssets
{
    /// <summary>
    /// Gets or sets the large image asset key.
    /// </summary>
    [JsonPropertyName("large_image")]
    public string? LargeImage { get; set; }

    /// <summary>
    /// Gets or sets the large image text.
    /// </summary>
    [JsonPropertyName("large_text")]
    public string? LargeText { get; set; }

    /// <summary>
    /// Gets or sets the small image asset key.
    /// </summary>
    [JsonPropertyName("small_image")]
    public string? SmallImage { get; set; }

    /// <summary>
    /// Gets or sets the small image text.
    /// </summary>
    [JsonPropertyName("small_text")]
    public string? SmallText { get; set; }
}
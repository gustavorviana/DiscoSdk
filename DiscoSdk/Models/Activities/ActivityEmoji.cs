using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Activities;

/// <summary>
/// Represents an emoji used in a custom status activity.
/// </summary>
public class ActivityEmoji
{
    /// <summary>
    /// Gets or sets the emoji name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the emoji ID.
    /// </summary>
    [JsonPropertyName("id")]
    public Snowflake? Id { get; set; }

    /// <summary>
    /// Gets or sets whether the emoji is animated.
    /// </summary>
    [JsonPropertyName("animated")]
    public bool? Animated { get; set; }
}

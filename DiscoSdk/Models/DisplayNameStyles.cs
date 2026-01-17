using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents display name styles for a user.
/// </summary>
public class DisplayNameStyles
{
    /// <summary>
    /// Gets or sets the font ID.
    /// </summary>
    [JsonPropertyName("font_id")]
    public int? FontId { get; set; }

    /// <summary>
    /// Gets or sets the effect ID.
    /// </summary>
    [JsonPropertyName("effect_id")]
    public int? EffectId { get; set; }

    /// <summary>
    /// Gets or sets the colors.
    /// </summary>
    [JsonPropertyName("colors")]
    public int[] Colors { get; set; } = [];
}


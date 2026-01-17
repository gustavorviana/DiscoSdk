using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents the color data of a Discord role.
/// </summary>
public class RoleColorData
{
    /// <summary>
    /// Gets or sets the primary color of the role.
    /// </summary>
    [JsonPropertyName("primary_color")]
    public Color? PrimaryColor { get; set; }

    /// <summary>
    /// Gets or sets the secondary color of the role.
    /// </summary>
    [JsonPropertyName("secondary_color")]
    public Color? SecondaryColor { get; set; }

    /// <summary>
    /// Gets or sets the tertiary color of the role.
    /// </summary>
    [JsonPropertyName("tertiary_color")]
    public Color? TertiaryColor { get; set; }
}

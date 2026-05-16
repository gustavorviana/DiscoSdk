using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Wire-format JSON DTO for the <c>colors</c> object inside a Discord role payload (legacy
/// holographic role colors). Hosting-internal: surfaced to the user only as primary/secondary/
/// tertiary values via <see cref="IRole"/>'s color accessors and <see cref="RoleColors"/>.
/// </summary>
internal class RoleColorData
{
    [JsonPropertyName("primary_color")]
    public Color? PrimaryColor { get; init; }

    [JsonPropertyName("secondary_color")]
    public Color? SecondaryColor { get; init; }

    [JsonPropertyName("tertiary_color")]
    public Color? TertiaryColor { get; init; }
}

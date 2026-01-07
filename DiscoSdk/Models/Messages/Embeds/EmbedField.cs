using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Embeds;

/// <summary>
/// Represents an embed field.
/// </summary>
public class EmbedField
{
    /// <summary>
    /// Gets or sets the name of the field.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the value of the field.
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; set; } = default!;

    /// <summary>
    /// Gets or sets a value indicating whether the field should be displayed inline.
    /// </summary>
    [JsonPropertyName("inline")]
    public bool? Inline { get; set; }
}

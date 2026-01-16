using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages;

/// <summary>
/// Represents a sticker item.
/// </summary>
public class StickerItem
{
    /// <summary>
    /// Gets or sets the ID of the sticker.
    /// </summary>
    [JsonPropertyName("id")]
    public Snowflake Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the name of the sticker.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the type of sticker format.
    /// </summary>
    [JsonPropertyName("format_type")]
    public StickerFormatType FormatType { get; set; }
}

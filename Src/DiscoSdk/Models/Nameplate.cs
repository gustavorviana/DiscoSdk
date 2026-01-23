using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a nameplate collectible.
/// </summary>
public class Nameplate
{
    /// <summary>
    /// Gets or sets the SKU ID.
    /// </summary>
    [JsonPropertyName("sku_id")]
    public Snowflake? SkuId { get; set; }

    /// <summary>
    /// Gets or sets the palette.
    /// </summary>
    [JsonPropertyName("palette")]
    public string? Palette { get; set; }

    /// <summary>
    /// Gets or sets the label.
    /// </summary>
    [JsonPropertyName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the expiration timestamp.
    /// </summary>
    [JsonPropertyName("expires_at")]
    public long? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the asset.
    /// </summary>
    [JsonPropertyName("asset")]
    public string? Asset { get; set; }
}

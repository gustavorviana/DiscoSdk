using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents avatar decoration data for a user.
/// </summary>
public class AvatarDecorationData
{
    /// <summary>
    /// Gets or sets the SKU ID.
    /// </summary>
    [JsonPropertyName("sku_id")]
    public Snowflake? SkuId { get; set; }

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


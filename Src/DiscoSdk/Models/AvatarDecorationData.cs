using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>Read-only user avatar-decoration metadata.</summary>
public class AvatarDecorationData
{
    /// <summary>SKU id of the decoration.</summary>
    [JsonPropertyName("sku_id")]
    public Snowflake? SkuId { get; init; }

    /// <summary>Unix-seconds timestamp when the decoration expires, or <c>null</c>.</summary>
    [JsonPropertyName("expires_at")]
    public long? ExpiresAt { get; init; }

    /// <summary>Decoration asset identifier.</summary>
    [JsonPropertyName("asset")]
    public string? Asset { get; init; }
}

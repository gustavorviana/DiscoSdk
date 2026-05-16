using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>Read-only nameplate-collectible metadata for a user.</summary>
public class Nameplate
{
    /// <summary>SKU id of the nameplate.</summary>
    [JsonPropertyName("sku_id")]
    public Snowflake? SkuId { get; init; }

    /// <summary>Palette identifier.</summary>
    [JsonPropertyName("palette")]
    public string? Palette { get; init; }

    /// <summary>Label string.</summary>
    [JsonPropertyName("label")]
    public string? Label { get; init; }

    /// <summary>Unix-seconds timestamp when the nameplate expires, or <c>null</c>.</summary>
    [JsonPropertyName("expires_at")]
    public long? ExpiresAt { get; init; }

    /// <summary>Nameplate asset identifier.</summary>
    [JsonPropertyName("asset")]
    public string? Asset { get; init; }
}

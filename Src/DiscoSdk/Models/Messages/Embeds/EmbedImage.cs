using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Embeds;

/// <summary>
/// Represents an embed image.
/// </summary>
public class EmbedImage
{
    /// <summary>
    /// Gets or sets the source URL of the image.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = default!;

    /// <summary>
    /// Gets or sets a proxied URL of the image.
    /// </summary>
    [JsonPropertyName("proxy_url")]
    public string? ProxyUrl { get; set; }

    /// <summary>
    /// Gets or sets the height of the image.
    /// </summary>
    [JsonPropertyName("height")]
    public int? Height { get; set; }

    /// <summary>
    /// Gets or sets the width of the image.
    /// </summary>
    [JsonPropertyName("width")]
    public int? Width { get; set; }
}
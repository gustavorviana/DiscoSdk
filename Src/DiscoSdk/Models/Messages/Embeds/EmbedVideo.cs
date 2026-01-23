using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Embeds;

/// <summary>
/// Represents an embed video.
/// </summary>
public class EmbedVideo
{
    /// <summary>
    /// Gets or sets the source URL of the video.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets a proxied URL of the video.
    /// </summary>
    [JsonPropertyName("proxy_url")]
    public string? ProxyUrl { get; set; }

    /// <summary>
    /// Gets or sets the height of the video.
    /// </summary>
    [JsonPropertyName("height")]
    public int? Height { get; set; }

    /// <summary>
    /// Gets or sets the width of the video.
    /// </summary>
    [JsonPropertyName("width")]
    public int? Width { get; set; }
}

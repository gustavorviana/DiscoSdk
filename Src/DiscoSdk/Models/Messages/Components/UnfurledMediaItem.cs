using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Reference to a media resource used by Components V2 (<see cref="ThumbnailComponent"/>,
/// <see cref="MediaGalleryItem"/>, <see cref="FileComponent"/>). Reference:
/// https://discord.com/developers/docs/components/reference#unfurled-media-item
/// </summary>
public class UnfurledMediaItem
{
    /// <summary>
    /// Supported sources: an arbitrary HTTPS URL, or an <c>attachment://&lt;filename&gt;</c> URI
    /// referring to a file attached in the same multipart message (the only form
    /// <see cref="FileComponent"/> accepts).
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = default!;

    /// <summary>Set by Discord on read — the proxied CDN URL.</summary>
    [JsonPropertyName("proxy_url")]
    public string? ProxyUrl { get; set; }

    [JsonPropertyName("height")]
    public int? Height { get; set; }

    [JsonPropertyName("width")]
    public int? Width { get; set; }

    [JsonPropertyName("content_type")]
    public string? ContentType { get; set; }

    /// <summary>Set by Discord on read for <c>attachment://</c> URLs.</summary>
    [JsonPropertyName("attachment_id")]
    public Snowflake? AttachmentId { get; set; }
}

using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages;

/// <summary>
/// Represents an attachment in a message.
/// </summary>
public class Attachment
{
    /// <summary>
    /// Gets or sets the attachment ID.
    /// </summary>
    [JsonPropertyName("id")]
    public Snowflake Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the name of the file attached.
    /// </summary>
    [JsonPropertyName("filename")]
    public string Filename { get; set; } = default!;

    /// <summary>
    /// Gets or sets the description for the file.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the attachment's media type.
    /// </summary>
    [JsonPropertyName("content_type")]
    public string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets the size of the file in bytes.
    /// </summary>
    [JsonPropertyName("size")]
    public int Size { get; set; }

    /// <summary>
    /// Gets or sets the source URL of the file.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = default!;

    /// <summary>
    /// Gets or sets a proxied URL of the file.
    /// </summary>
    [JsonPropertyName("proxy_url")]
    public string ProxyUrl { get; set; } = default!;

    /// <summary>
    /// Gets or sets the height of the file (if image).
    /// </summary>
    [JsonPropertyName("height")]
    public int? Height { get; set; }

    /// <summary>
    /// Gets or sets the width of the file (if image).
    /// </summary>
    [JsonPropertyName("width")]
    public int? Width { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this attachment is ephemeral.
    /// </summary>
    [JsonPropertyName("ephemeral")]
    public bool? Ephemeral { get; set; }

    /// <summary>
    /// Gets or sets the duration of the audio file (if audio).
    /// </summary>
    [JsonPropertyName("duration_secs")]
    public double? DurationSecs { get; set; }

    /// <summary>
    /// Gets or sets the base64 encoded bytearray representing a sampled waveform (if audio).
    /// </summary>
    [JsonPropertyName("waveform")]
    public string? Waveform { get; set; }
}


using DiscoSdk.Models.JsonConverters;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Embeds;

/// <summary>
/// Represents an embed in a message.
/// </summary>
public class Embed
{
    /// <summary>
    /// Gets or sets the title of the embed.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the type of embed.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the description of the embed.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the URL of the embed.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the embed content.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the color code of the embed.
    /// </summary>
    [JsonPropertyName("color")]
    public Color? Color { get; set; }

    /// <summary>
    /// Gets or sets the footer information.
    /// </summary>
    [JsonPropertyName("footer")]
    public EmbedFooter? Footer { get; set; }

    /// <summary>
    /// Gets or sets the image information.
    /// </summary>
    [JsonPropertyName("image")]
    public EmbedImage? Image { get; set; }

    /// <summary>
    /// Gets or sets the thumbnail information.
    /// </summary>
    [JsonPropertyName("thumbnail")]
    public EmbedThumbnail? Thumbnail { get; set; }

    /// <summary>
    /// Gets or sets the video information.
    /// </summary>
    [JsonPropertyName("video")]
    public EmbedVideo? Video { get; set; }

    /// <summary>
    /// Gets or sets the provider information.
    /// </summary>
    [JsonPropertyName("provider")]
    public EmbedProvider? Provider { get; set; }

    /// <summary>
    /// Gets or sets the author information.
    /// </summary>
    [JsonPropertyName("author")]
    public EmbedAuthor? Author { get; set; }

    /// <summary>
    /// Gets or sets the fields of the embed.
    /// </summary>
    [JsonPropertyName("fields")]
    public EmbedField[]? Fields { get; set; }
}


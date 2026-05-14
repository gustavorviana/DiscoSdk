using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Single entry inside a <see cref="MediaGalleryComponent.Items"/> array. Reference:
/// https://discord.com/developers/docs/components/reference#media-gallery-item
/// </summary>
public class MediaGalleryItem
{
    /// <summary>The image (or video / animated image) to display.</summary>
    [JsonPropertyName("media")]
    public UnfurledMediaItem Media { get; set; } = default!;

    /// <summary>Alt text shown on hover / by screen readers.</summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }

    /// <summary>If <c>true</c>, the item is hidden behind a spoiler overlay.</summary>
    [JsonPropertyName("spoiler")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Spoiler { get; set; }
}

using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Grid of 1–10 media items rendered as a compact gallery. Reference:
/// https://discord.com/developers/docs/components/reference#media-gallery
/// </summary>
public sealed class MediaGalleryComponent : IMessageComponent
{
    /// <inheritdoc />
    [JsonPropertyName("type")]
    public ComponentType Type { get; set; } = ComponentType.MediaGallery;

    /// <inheritdoc />
    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Id { get; set; }

    /// <summary>1–10 media items.</summary>
    [JsonPropertyName("items")]
    public MediaGalleryItem[] Items { get; set; } = [];

    /// <inheritdoc />
    [JsonIgnore]
    public string? CustomId { get; set; }

    /// <inheritdoc />
    [JsonIgnore]
    public bool? Disabled { get; set; }
}

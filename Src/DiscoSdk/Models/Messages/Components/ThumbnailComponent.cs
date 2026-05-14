using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Small image used as the <see cref="SectionComponent.Accessory"/> of a <see cref="SectionComponent"/>
/// (the only place this component is valid). Reference:
/// https://discord.com/developers/docs/components/reference#thumbnail
/// </summary>
public sealed class ThumbnailComponent : IMessageComponent
{
    /// <inheritdoc />
    [JsonPropertyName("type")]
    public ComponentType Type { get; set; } = ComponentType.Thumbnail;

    /// <inheritdoc />
    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Id { get; set; }

    /// <summary>The image to render. HTTPS URL or <c>attachment://...</c>.</summary>
    [JsonPropertyName("media")]
    public UnfurledMediaItem Media { get; set; } = default!;

    /// <summary>Alt text shown to screen readers and on hover. Up to 1 024 chars.</summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }

    /// <summary>If <c>true</c>, render as a spoiler — viewer must click to reveal.</summary>
    [JsonPropertyName("spoiler")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Spoiler { get; set; }

    /// <inheritdoc />
    [JsonIgnore]
    public string? CustomId { get; set; }

    /// <inheritdoc />
    [JsonIgnore]
    public bool? Disabled { get; set; }
}

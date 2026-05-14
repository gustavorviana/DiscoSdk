using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Outer wrapper with an optional accent color and spoiler around a group of other V2 components.
/// Valid children: <see cref="ActionRowComponent"/>, <see cref="TextDisplayComponent"/>,
/// <see cref="SectionComponent"/>, <see cref="MediaGalleryComponent"/>, <see cref="FileComponent"/>,
/// <see cref="SeparatorComponent"/>. Reference:
/// https://discord.com/developers/docs/components/reference#container
/// </summary>
public sealed class ContainerComponent : IMessageComponent
{
    /// <inheritdoc />
    [JsonPropertyName("type")]
    public ComponentType Type { get; set; } = ComponentType.Container;

    /// <inheritdoc />
    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Id { get; set; }

    /// <summary>Components rendered inside the container.</summary>
    [JsonPropertyName("components")]
    [JsonConverter(typeof(InteractionComponentConverter))]
    public IInteractionComponent[] Components { get; set; } = [];

    /// <summary>RGB integer drawn as a left-edge accent bar.</summary>
    [JsonPropertyName("accent_color")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? AccentColor { get; set; }

    /// <summary>If <c>true</c>, the entire container is hidden behind a spoiler overlay.</summary>
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

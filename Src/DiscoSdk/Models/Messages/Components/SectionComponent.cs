using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Container that pairs 1–3 <see cref="TextDisplayComponent"/> blocks with an accessory (a
/// <see cref="ButtonComponent"/> or a <see cref="ThumbnailComponent"/>) rendered alongside.
/// Reference: https://discord.com/developers/docs/components/reference#section
/// </summary>
public sealed class SectionComponent : IMessageComponent
{
    /// <inheritdoc />
    [JsonPropertyName("type")]
    public ComponentType Type { get; set; } = ComponentType.Section;

    /// <inheritdoc />
    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Id { get; set; }

    /// <summary>1–3 <see cref="TextDisplayComponent"/> blocks.</summary>
    [JsonPropertyName("components")]
    public TextDisplayComponent[] Components { get; set; } = [];

    /// <summary>
    /// The accessory rendered next to the text. Must be a <see cref="ButtonComponent"/> or a
    /// <see cref="ThumbnailComponent"/>.
    /// </summary>
    [JsonPropertyName("accessory")]
    [JsonConverter(typeof(MessageComponentPolymorphicConverter))]
    public IMessageComponent Accessory { get; set; } = default!;

    /// <inheritdoc />
    [JsonIgnore]
    public string? CustomId { get; set; }

    /// <inheritdoc />
    [JsonIgnore]
    public bool? Disabled { get; set; }
}

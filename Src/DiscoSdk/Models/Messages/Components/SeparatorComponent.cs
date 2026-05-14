using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Horizontal spacing — optionally with a visible divider line — between other components.
/// Reference: https://discord.com/developers/docs/components/reference#separator
/// </summary>
public sealed class SeparatorComponent : IMessageComponent
{
    /// <inheritdoc />
    [JsonPropertyName("type")]
    public ComponentType Type { get; set; } = ComponentType.Separator;

    /// <inheritdoc />
    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Id { get; set; }

    /// <summary>
    /// If <c>true</c>, a visible horizontal line is drawn. If <c>false</c>, only spacing is added.
    /// </summary>
    [JsonPropertyName("divider")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Divider { get; set; }

    /// <summary>Padding size around the separator. Defaults to <see cref="SeparatorSpacing.Small"/>.</summary>
    [JsonPropertyName("spacing")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SeparatorSpacing? Spacing { get; set; }

    /// <inheritdoc />
    [JsonIgnore]
    public string? CustomId { get; set; }

    /// <inheritdoc />
    [JsonIgnore]
    public bool? Disabled { get; set; }
}

using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Markdown text block — the Components V2 equivalent of message <c>content</c>. Up to 4 000
/// characters; supports the same Markdown subset as <c>content</c>. Reference:
/// https://discord.com/developers/docs/components/reference#text-display
/// </summary>
public sealed class TextDisplayComponent : IMessageComponent
{
    /// <inheritdoc />
    [JsonPropertyName("type")]
    public ComponentType Type { get; set; } = ComponentType.TextDisplay;

    /// <inheritdoc />
    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Id { get; set; }

    /// <summary>The Markdown content to display.</summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = default!;

    /// <inheritdoc />
    [JsonIgnore]
    public string? CustomId { get; set; }

    /// <inheritdoc />
    [JsonIgnore]
    public bool? Disabled { get; set; }
}

using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Represents a message component.
/// </summary>
public class MessageComponent : IInteractionComponent
{
    /// <summary>
    /// Gets or sets the type of component.
    /// </summary>
    [JsonPropertyName("type")]
    public ComponentType Type { get; set; }

    /// <summary>
    /// Gets or sets the style of the component.
    /// </summary>
    [JsonPropertyName("style")]
    public ButtonStyle? Style { get; set; }

    /// <summary>
    /// Gets or sets the label of the component.
    /// </summary>
    [JsonPropertyName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the emoji of the component.
    /// </summary>
    [JsonPropertyName("emoji")]
    public Emoji? Emoji { get; set; }

    /// <summary>
    /// Gets or sets the custom ID of the component.
    /// </summary>
    [JsonPropertyName("custom_id")]
    public string? CustomId { get; set; }

    /// <summary>
    /// Gets or sets the URL of the component.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the component is disabled.
    /// </summary>
    [JsonPropertyName("disabled")]
    public bool? Disabled { get; set; }

    /// <summary>
    /// Gets or sets the components of the component.
    /// </summary>
    [JsonPropertyName("components")]
    public MessageComponent[]? Components { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text for select components.
    /// </summary>
    [JsonPropertyName("placeholder")]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the minimum number of items that must be chosen.
    /// </summary>
    [JsonPropertyName("min_values")]
    public int? MinValues { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of items that can be chosen.
    /// </summary>
    [JsonPropertyName("max_values")]
    public int? MaxValues { get; set; }

    /// <summary>
    /// Gets or sets the options for select components.
    /// </summary>
    [JsonPropertyName("options")]
    public SelectOption[]? Options { get; set; }
}

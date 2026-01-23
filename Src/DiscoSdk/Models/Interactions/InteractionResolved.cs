using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Interactions;

/// <summary>
/// Represents a text input component submitted in a modal.
/// </summary>
public class ModalTextInput
{
    /// <summary>
    /// Gets or sets the type of component. Must be TextInput (4).
    /// </summary>
    [JsonPropertyName("type")]
    public ComponentType Type { get; set; } = ComponentType.TextInput;

    /// <summary>
    /// Gets or sets the custom ID of the text input.
    /// </summary>
    [JsonPropertyName("custom_id")]
    public string CustomId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the value entered by the user.
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; set; } = default!;
}
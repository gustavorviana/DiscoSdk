using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Interactions;

/// <summary>
/// Represents a component submitted in a modal.
/// </summary>
public class ModalComponent
{
    /// <summary>
    /// Gets or sets the type of component. Must be ActionRow (1) for modal submissions.
    /// </summary>
    [JsonPropertyName("type")]
    public ComponentType Type { get; set; } = ComponentType.ActionRow;

    /// <summary>
    /// Gets or sets the components within this action row (text inputs).
    /// </summary>
    [JsonPropertyName("components")]
    public ModalTextInput[]? Components { get; set; }
}


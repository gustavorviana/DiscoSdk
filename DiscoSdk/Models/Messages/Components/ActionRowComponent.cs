using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Represents an action row component that can contain text inputs in a modal.
/// </summary>
public class ActionRowComponent : IInteractionComponent
{
    /// <summary>
    /// Gets or sets the type of component. Must be ActionRow (1) for modals.
    /// </summary>
    [JsonPropertyName("type")]
    public ComponentType Type { get; set; } = ComponentType.ActionRow;

    /// <summary>
    /// Gets or sets the components within this action row. Must contain exactly one TextInput component.
    /// </summary>
    [JsonPropertyName("components")]
    public TextInputComponent[] Components { get; set; } = [];
}


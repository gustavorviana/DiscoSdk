using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Represents an action row component that can contain text inputs in a modal. Checkbox Group must use <see cref="LabelComponent"/> (type 18), not ActionRow.
/// </summary>
public class ActionRowComponent : IModalComponent
{
	/// <summary>
	/// Gets or sets the type of component. Must be ActionRow (1) for modals.
	/// </summary>
	[JsonPropertyName("type")]
	public ComponentType Type { get; set; } = ComponentType.ActionRow;

	/// <summary>
	/// Gets or sets the optional identifier for the component. Omitted when null per Discord API.
	/// </summary>
	[JsonPropertyName("id")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? Id { get; set; }

	/// <summary>
	/// Gets or sets the components within this action row (one TextInput only; Checkbox Group uses Label container).
	/// </summary>
	[JsonPropertyName("components")]
	[JsonConverter(typeof(ActionRowModalComponentConverter))]
	public IModalComponent[] Components { get; set; } = [];
}


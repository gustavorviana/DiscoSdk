using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Top-level layout component for modals that wraps a label, optional description, and a single child component (e.g. Checkbox Group).
/// In modals, Checkbox Group must be inside a Label (type 18), not inside an ActionRow (type 1).
/// </summary>
public class LabelComponent : IModalComponent
{
	/// <summary>
	/// Gets or sets the type of component. Must be Label (18).
	/// </summary>
	[JsonPropertyName("type")]
	public ComponentType Type { get; set; } = ComponentType.Label;

	/// <summary>
	/// Gets or sets the optional identifier for the component. Omitted when null per Discord API.
	/// </summary>
	[JsonPropertyName("id")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? Id { get; set; }

	/// <summary>
	/// Gets or sets the label text (max 45 characters).
	/// </summary>
	[JsonPropertyName("label")]
	public string Label { get; set; } = default!;

	/// <summary>
	/// Gets or sets the optional description text (max 100 characters). Omitted when null per Discord API.
	/// </summary>
	[JsonPropertyName("description")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Description { get; set; }

	/// <summary>
	/// Gets or sets the single child component (e.g. CheckboxGroupComponent). Required for Label in modals.
	/// Serialized with concrete type so Discord receives type, custom_id and options for type 22.
	/// </summary>
	[JsonPropertyName("component")]
	[JsonConverter(typeof(ModalChildComponentConverter))]
	public IModalComponent? Component { get; set; }
}

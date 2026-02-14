using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Interactive component for selecting exactly one option from a defined list in a modal.
/// Must be placed inside a <see cref="LabelComponent"/>.
/// </summary>
public class RadioGroupComponent : IModalComponent
{
	/// <summary>
	/// Gets or sets the type of component. Must be RadioGroup (21).
	/// </summary>
	[JsonPropertyName("type")]
	public ComponentType Type { get; set; } = ComponentType.RadioGroup;

	/// <summary>
	/// Gets or sets the optional identifier for the component.
	/// </summary>
	[JsonPropertyName("id")]
	public int? Id { get; set; }

	/// <summary>
	/// Gets or sets the developer-defined identifier for the input (1-100 characters).
	/// </summary>
	[JsonPropertyName("custom_id")]
	public string CustomId { get; set; } = default!;

	/// <summary>
	/// Gets or sets the list of options to show (min 2, max 10).
	/// </summary>
	[JsonPropertyName("options")]
	public RadioGroupOption[] Options { get; set; } = [];

	/// <summary>
	/// Gets or sets whether a selection is required to submit the modal. Defaults to true.
	/// </summary>
	[JsonPropertyName("required")]
	public bool? Required { get; set; }
}

using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Single interactive component for yes/no style questions in a modal.
/// Must be placed inside a <see cref="LabelComponent"/>.
/// </summary>
public class CheckboxComponent : IModalComponent
{
	/// <summary>
	/// Gets or sets the type of component. Must be Checkbox (23).
	/// </summary>
	[JsonPropertyName("type")]
	public ComponentType Type { get; set; } = ComponentType.Checkbox;

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
	/// Gets or sets whether the checkbox is selected by default.
	/// </summary>
	[JsonPropertyName("default")]
	public bool? Default { get; set; }
}

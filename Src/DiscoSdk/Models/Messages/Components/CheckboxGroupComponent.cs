using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Interactive component for selecting zero to many options via checkboxes in a modal.
/// Must be placed inside a <see cref="LabelComponent"/> (type 18) when used in modals; the SDK wraps it automatically when added via <see cref="Rest.Actions.IReplyModalRestAction.AddActionRow"/>.
/// </summary>
public class CheckboxGroupComponent : IModalComponent
{
	/// <summary>
	/// Gets or sets the type of component. Must be CheckboxGroup (22).
	/// </summary>
	[JsonPropertyName("type")]
	public ComponentType Type { get; set; } = ComponentType.CheckboxGroup;

	/// <summary>
	/// Gets or sets the optional identifier for the component. Omitted when null per Discord API.
	/// </summary>
	[JsonPropertyName("id")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? Id { get; set; }

	/// <summary>
	/// Gets or sets the developer-defined identifier for the input (1-100 characters).
	/// </summary>
	[JsonPropertyName("custom_id")]
	public string CustomId { get; set; } = default!;

	/// <summary>
	/// Gets or sets the list of options to show (min 1, max 10).
	/// </summary>
	[JsonPropertyName("options")]
	public CheckboxGroupOption[] Options { get; set; } = [];

	/// <summary>
	/// Gets or sets the minimum number of items that must be chosen (0-10). Defaults to 1.
	/// If set to 0, <see cref="Required"/> must be false.
	/// </summary>
	[JsonPropertyName("min_values")]
	public int? MinValues { get; set; }

	/// <summary>
	/// Gets or sets the maximum number of items that can be chosen (1-10). Defaults to the number of options. Omitted when null.
	/// </summary>
	[JsonPropertyName("max_values")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? MaxValues { get; set; }

	/// <summary>
	/// Gets or sets whether selecting within the group is required. Defaults to true.
	/// </summary>
	[JsonPropertyName("required")]
	public bool? Required { get; set; }

	/// <summary>
	/// Gets or sets the label text for the Label container (type 18) when this component is used in a modal (max 45 characters).
	/// Required when adding a CheckboxGroup to a modal via <see cref="Rest.Actions.IReplyModalRestAction.AddActionRow"/>.
	/// </summary>
	[JsonIgnore]
	public string? Label { get; set; }
}

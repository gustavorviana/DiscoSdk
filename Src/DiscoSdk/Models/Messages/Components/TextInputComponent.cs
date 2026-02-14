using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Represents a text input component for modals.
/// </summary>
public class TextInputComponent : IModalComponent
{
	/// <summary>
	/// Gets or sets the type of component. Must be TextInput (4).
	/// </summary>
	[JsonPropertyName("type")]
	public ComponentType Type { get; set; } = ComponentType.TextInput;

	/// <summary>
	/// Gets or sets the optional identifier for the component.
	/// </summary>
	[JsonPropertyName("id")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? Id { get; set; }

	/// <summary>
	/// Gets or sets the custom ID for the text input (max 100 characters).
	/// </summary>
	[JsonPropertyName("custom_id")]
	public string CustomId { get; set; } = default!;

	/// <summary>
	/// Gets or sets the style of the text input.
	/// 1 = Short (single line), 2 = Paragraph (multi-line).
	/// </summary>
	[JsonPropertyName("style")]
	public TextInputStyle Style { get; set; } = TextInputStyle.Short;

	/// <summary>
	/// Gets or sets the label for the text input (max 45 characters).
	/// </summary>
	[JsonPropertyName("label")]
	public string Label { get; set; } = default!;

	/// <summary>
	/// Gets or sets the minimum length of the input (1-4000). Defaults to 0.
	/// </summary>
	[JsonPropertyName("min_length")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? MinLength { get; set; }

	/// <summary>
	/// Gets or sets the maximum length of the input (1-4000). Defaults to 4000.
	/// </summary>
	[JsonPropertyName("max_length")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? MaxLength { get; set; }

	/// <summary>
	/// Gets or sets the placeholder text if the input is empty (max 100 characters).
	/// </summary>
	[JsonPropertyName("placeholder")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Placeholder { get; set; }

	/// <summary>
	/// Gets or sets whether the input is required. Defaults to false.
	/// </summary>
	[JsonPropertyName("required")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public bool? Required { get; set; }

	/// <summary>
	/// Gets or sets the value of the text input (pre-filled value).
	/// </summary>
	[JsonPropertyName("value")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Value { get; set; }
}
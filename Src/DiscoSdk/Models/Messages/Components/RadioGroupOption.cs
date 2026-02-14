using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Represents an option in a radio group component.
/// </summary>
public class RadioGroupOption
{
	/// <summary>
	/// Gets or sets the dev-defined value of the option (max 100 characters).
	/// </summary>
	[JsonPropertyName("value")]
	public string Value { get; set; } = default!;

	/// <summary>
	/// Gets or sets the user-facing label of the option (max 100 characters).
	/// </summary>
	[JsonPropertyName("label")]
	public string Label { get; set; } = default!;

	/// <summary>
	/// Gets or sets the optional description for the option (max 100 characters).
	/// </summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether this option is selected by default.
	/// </summary>
	[JsonPropertyName("default")]
	public bool? Default { get; set; }
}

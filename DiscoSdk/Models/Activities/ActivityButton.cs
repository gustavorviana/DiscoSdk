using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Activities;

/// <summary>
/// Represents a button in an activity.
/// </summary>
public class ActivityButton
{
	/// <summary>
	/// Gets or sets the button label.
	/// </summary>
	[JsonPropertyName("label")]
	public string Label { get; set; } = default!;

	/// <summary>
	/// Gets or sets the button URL.
	/// </summary>
	[JsonPropertyName("url")]
	public string Url { get; set; } = default!;
}
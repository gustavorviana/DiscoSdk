using DiscoSdk.Models.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Interactions;

/// <summary>
/// One submitted field in a modal submit payload (type, custom_id, value/values). For building modals use <see cref="Models.Messages.Components.IModalComponent"/>.
/// </summary>
public class ModalSubmitField
{
	[JsonPropertyName("type")]
	public ComponentType? Type { get; set; }

	[JsonPropertyName("id")]
	public int? Id { get; set; }

	[JsonPropertyName("custom_id")]
	public string CustomId { get; set; } = default!;

	[JsonPropertyName("value")]
	public JsonElement? Value { get; set; }

	[JsonPropertyName("values")]
	public string[]? Values { get; set; }

	/// <summary>
	/// Submitted value as a single string. Multi-value: comma-separated; checkbox: "True"/"False".
	/// </summary>
	public string? GetValueString()
	{
		if (Values != null && Values.Length > 0)
			return string.Join(",", Values);
		if (Value is not { } v)
			return null;
		return v.ValueKind switch
		{
			JsonValueKind.String => v.GetString(),
			JsonValueKind.True => "True",
			JsonValueKind.False => "False",
			_ => null
		};
	}
}

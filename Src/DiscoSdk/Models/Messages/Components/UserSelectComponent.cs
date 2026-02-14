using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// User select component for messages and threads.
/// </summary>
public class UserSelectComponent : IMessageComponent
{
	[JsonPropertyName("type")]
	public ComponentType Type { get; set; } = ComponentType.UserSelect;

	[JsonPropertyName("id")]
	public int? Id { get; set; }

	[JsonPropertyName("custom_id")]
	public string? CustomId { get; set; }

	[JsonPropertyName("placeholder")]
	public string? Placeholder { get; set; }

	[JsonPropertyName("default_values")]
	public SelectDefaultValue[]? DefaultValues { get; set; }

	[JsonPropertyName("min_values")]
	public int? MinValues { get; set; }

	[JsonPropertyName("max_values")]
	public int? MaxValues { get; set; }

	[JsonPropertyName("disabled")]
	public bool? Disabled { get; set; }
}

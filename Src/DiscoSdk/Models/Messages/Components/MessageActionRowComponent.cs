using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Action row for messages and threads; contains buttons or a single select.
/// </summary>
public class MessageActionRowComponent : IMessageComponent
{
	[JsonPropertyName("type")]
	public ComponentType Type { get; set; } = ComponentType.ActionRow;

	[JsonPropertyName("id")]
	public int? Id { get; set; }

	[JsonPropertyName("custom_id")]
	public string? CustomId { get; set; }

	[JsonPropertyName("disabled")]
	public bool? Disabled { get; set; }

	[JsonPropertyName("components")]
	public IMessageComponent[] Components { get; set; } = [];
}

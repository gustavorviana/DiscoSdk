using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Action row for messages and threads; contains buttons or a single select. The inner
/// <see cref="Components"/> array uses <see cref="InteractionComponentConverter"/> for the
/// same polymorphic discrimination as the outer components array on a message.
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
	[JsonConverter(typeof(InteractionComponentConverter))]
	public IInteractionComponent[] Components { get; set; } = [];
}

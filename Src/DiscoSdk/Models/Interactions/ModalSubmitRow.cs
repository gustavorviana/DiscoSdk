using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Interactions;

/// <summary>
/// One ActionRow (type 1) in the modal submit payload from Discord. For building modals use <see cref="Models.Messages.Components.IModalComponent"/>.
/// </summary>
public class ModalSubmitRow
{
	[JsonPropertyName("type")]
	public ComponentType Type { get; set; } = ComponentType.ActionRow;

	[JsonPropertyName("id")]
	public int? Id { get; set; }

	[JsonPropertyName("components")]
	public ModalSubmitField[]? Components { get; set; }
}

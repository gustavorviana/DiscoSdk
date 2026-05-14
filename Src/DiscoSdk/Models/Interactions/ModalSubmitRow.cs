using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Interactions;

/// <summary>
/// One top-level component in the modal submit payload from Discord. Holds either an
/// <c>ActionRow</c> (with <see cref="Components"/> = array of TextInput results) or a
/// <c>Label</c> (with <see cref="Component"/> = single child result — Checkbox / CheckboxGroup /
/// RadioGroup / FileUpload). The receiving side flattens both into the same option list.
/// For building modals use <see cref="Models.Messages.Components.IModalComponent"/>.
/// </summary>
public class ModalSubmitRow
{
	[JsonPropertyName("type")]
	public ComponentType Type { get; set; } = ComponentType.ActionRow;

	[JsonPropertyName("id")]
	public int? Id { get; set; }

	/// <summary>ActionRow children (<c>type</c>=1). Null for Label-typed rows.</summary>
	[JsonPropertyName("components")]
	public ModalSubmitField[]? Components { get; set; }

	/// <summary>Single Label child (<c>type</c>=18 → checkbox / radio / file-upload). Null for ActionRow rows.</summary>
	[JsonPropertyName("component")]
	public ModalSubmitField? Component { get; set; }
}

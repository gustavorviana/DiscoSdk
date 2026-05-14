using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Modal-exclusive component that lets the user attach 1–10 files when submitting a modal.
/// Like <see cref="CheckboxComponent"/> / <see cref="CheckboxGroupComponent"/> / <see cref="RadioGroupComponent"/>,
/// must live inside a <see cref="LabelComponent"/> (type 18); the SDK auto-wraps it when added via
/// <see cref="Rest.Actions.IReplyModalRestAction.AddActionRow"/>.
/// Reference: https://docs.discord.com/developers/components/reference#file-upload
/// </summary>
public class FileUploadComponent : IModalComponent
{
	/// <summary>Component type — always <c>FileUpload</c> (19).</summary>
	[JsonPropertyName("type")]
	public ComponentType Type { get; set; } = ComponentType.FileUpload;

	/// <summary>Optional 32-bit identifier.</summary>
	[JsonPropertyName("id")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? Id { get; set; }

	/// <summary>Developer-defined identifier (1–100 chars). Required.</summary>
	[JsonPropertyName("custom_id")]
	public string CustomId { get; set; } = default!;

	/// <summary>Minimum number of files the user must attach (defaults to 1). Range 0–10.</summary>
	[JsonPropertyName("min_values")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? MinValues { get; set; }

	/// <summary>Maximum number of files the user can attach (defaults to 1). Range 1–10.</summary>
	[JsonPropertyName("max_values")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? MaxValues { get; set; }

	/// <summary>Whether the field is required (defaults to true).</summary>
	[JsonPropertyName("required")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public bool? Required { get; set; }

	/// <summary>
	/// Label text shown by the wrapping <see cref="LabelComponent"/> when added via
	/// <see cref="Rest.Actions.IReplyModalRestAction.AddActionRow"/> (max 45 chars). Required.
	/// </summary>
	[JsonIgnore]
	public string? Label { get; set; }

	/// <summary>Optional secondary text shown below the label (max 100 chars).</summary>
	[JsonIgnore]
	public string? Description { get; set; }
}

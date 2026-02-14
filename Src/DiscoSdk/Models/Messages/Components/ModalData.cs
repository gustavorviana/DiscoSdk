using DiscoSdk.Models.JsonConverters;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Represents modal data for responding to an interaction with a modal.
/// </summary>
public class ModalData
{
	/// <summary>
	/// Gets or sets the custom ID for the modal (max 100 characters).
	/// </summary>
	[JsonPropertyName("custom_id")]
	public string CustomId { get; set; } = default!;

	/// <summary>
	/// Gets or sets the title of the modal (max 45 characters).
	/// </summary>
	[JsonPropertyName("title")]
	public string Title { get; set; } = default!;

	/// <summary>
	/// Gets or sets the components in the modal (ActionRow or Label).
	/// </summary>
	[JsonPropertyName("components")]
	[JsonConverter(typeof(ModalComponentConverter))]
	public IModalComponent[] Components { get; set; } = [];
}

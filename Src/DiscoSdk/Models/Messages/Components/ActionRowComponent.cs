using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Action row (type 1) for <strong>modal</strong> contexts — holds a single
/// <see cref="TextInputComponent"/>. For message-context action rows that hold buttons or
/// selects, use <see cref="MessageActionRowComponent"/> instead. Both serialize as
/// <c>type:1</c> on the wire but have different child rules; the SDK keeps them as separate
/// classes so polymorphic deserialization can pick the correct shape per context.
/// </summary>
public class ActionRowComponent : IModalComponent
{
	/// <summary>
	/// Gets or sets the type of component. Must be ActionRow (1) for modals.
	/// </summary>
	[JsonPropertyName("type")]
	public ComponentType Type { get; set; } = ComponentType.ActionRow;

	/// <summary>
	/// Gets or sets the optional identifier for the component. Omitted when null per Discord API.
	/// </summary>
	[JsonPropertyName("id")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? Id { get; set; }

	/// <summary>
	/// Gets or sets the components within this action row (one TextInput only; Checkbox Group uses Label container).
	/// </summary>
	[JsonPropertyName("components")]
	[JsonConverter(typeof(ActionRowModalComponentConverter))]
	public IModalComponent[] Components { get; set; } = [];
}


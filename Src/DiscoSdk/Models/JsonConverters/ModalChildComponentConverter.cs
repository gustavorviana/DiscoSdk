using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages.Components;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.JsonConverters;

/// <summary>
/// JSON converter for the child <see cref="IModalComponent"/> of a <see cref="LabelComponent"/> (e.g. CheckboxGroup type 22).
/// Ensures the child is serialized with its concrete type so required fields (type, custom_id, options) are always present.
/// </summary>
public class ModalChildComponentConverter : JsonConverter<IModalComponent?>
{
	/// <inheritdoc />
	public override IModalComponent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		using var doc = JsonDocument.ParseValue(ref reader);
		var root = doc.RootElement;

		if (!root.TryGetProperty("type", out var typeElement))
			throw new JsonException("Component missing 'type' field.");

		var componentType = (ComponentType)typeElement.GetInt32();

		return componentType switch
		{
			ComponentType.CheckboxGroup => JsonSerializer.Deserialize<CheckboxGroupComponent>(root.GetRawText(), options)
				?? throw new JsonException("Failed to deserialize CheckboxGroupComponent."),
			_ => throw new JsonException($"Label component child type {componentType} is not supported for modal.")
		};
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, IModalComponent? value, JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}

		JsonSerializer.Serialize(writer, value, value.GetType(), options);
	}
}

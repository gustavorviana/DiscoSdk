using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages.Components;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.JsonConverters;

/// <summary>
/// JSON converter for <see cref="IModalComponent"/> arrays (modal payload).
/// Supports ActionRow (type 1) for TextInput and Label (type 18) for Checkbox Group and similar components per Discord API.
/// </summary>
public class ModalComponentConverter : JsonConverter<IModalComponent[]>
{
	/// <inheritdoc />
	public override IModalComponent[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType != JsonTokenType.StartArray)
			throw new JsonException("Expected start of array.");

		var components = new List<IModalComponent>();

		while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
		{
			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			if (!root.TryGetProperty("type", out var typeElement))
				throw new JsonException("Component missing 'type' field.");

			var componentType = (ComponentType)typeElement.GetInt32();

			IModalComponent component = componentType switch
			{
				ComponentType.ActionRow => JsonSerializer.Deserialize<ActionRowComponent>(root.GetRawText(), options)
					?? throw new JsonException("Failed to deserialize ActionRowComponent."),
				ComponentType.Label => JsonSerializer.Deserialize<LabelComponent>(root.GetRawText(), options)
					?? throw new JsonException("Failed to deserialize LabelComponent."),
				_ => throw new JsonException($"Modal components must be ActionRow (type 1) or Label (type 18). Unexpected type: {componentType}.")
			};
			components.Add(component);
		}

		return components.ToArray();
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, IModalComponent[]? value, JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteStartArray();
		foreach (var component in value)
			JsonSerializer.Serialize(writer, component, component.GetType(), options);
		writer.WriteEndArray();
	}
}

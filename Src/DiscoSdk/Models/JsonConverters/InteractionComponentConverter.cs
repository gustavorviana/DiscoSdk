using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages.Components;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.JsonConverters;

/// <summary>
/// JSON converter for <see cref="IInteractionComponent"/> arrays.
/// Handles serialization and deserialization of both <see cref="MessageComponent"/> and <see cref="ActionRowComponent"/>.
/// </summary>
public class InteractionComponentConverter : JsonConverter<IInteractionComponent[]>
{
	/// <inheritdoc />
	public override IInteractionComponent[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType != JsonTokenType.StartArray)
			throw new JsonException("Expected start of array.");

		var components = new List<IInteractionComponent>();

		while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
		{
			// Read the entire object into a JsonDocument to inspect the "type" field
			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			if (!root.TryGetProperty("type", out var typeElement))
				throw new JsonException("Component missing 'type' field.");

			var componentType = (ComponentType)typeElement.GetInt32();

			// Deserialize based on component type
			IInteractionComponent component = componentType switch
			{
				ComponentType.ActionRow => JsonSerializer.Deserialize<ActionRowComponent>(root.GetRawText(), options) 
					?? throw new JsonException("Failed to deserialize ActionRowComponent."),
				_ => JsonSerializer.Deserialize<MessageComponent>(root.GetRawText(), options) 
					?? throw new JsonException("Failed to deserialize MessageComponent.")
			};

			components.Add(component);
		}

		return components.ToArray();
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, IInteractionComponent[]? value, JsonSerializerOptions options)
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


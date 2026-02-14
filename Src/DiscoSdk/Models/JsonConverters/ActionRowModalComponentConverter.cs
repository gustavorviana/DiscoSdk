using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages.Components;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.JsonConverters;

/// <summary>
/// JSON converter for the inner components array of an ActionRow in a modal (TextInput or CheckboxGroup).
/// </summary>
public class ActionRowModalComponentConverter : JsonConverter<IModalComponent[]>
{
	/// <inheritdoc />
	public override IModalComponent[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType != JsonTokenType.StartArray)
			throw new JsonException("Expected start of array.");

		var list = new List<IModalComponent>();

		while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
		{
			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			if (!root.TryGetProperty("type", out var typeEl))
				throw new JsonException("Component missing 'type' field.");

			var type = (ComponentType)typeEl.GetInt32();
			IModalComponent component = type switch
			{
				ComponentType.TextInput => JsonSerializer.Deserialize<TextInputComponent>(root.GetRawText(), options)
					?? throw new JsonException("Failed to deserialize TextInputComponent."),
				ComponentType.CheckboxGroup => JsonSerializer.Deserialize<CheckboxGroupComponent>(root.GetRawText(), options)
					?? throw new JsonException("Failed to deserialize CheckboxGroupComponent."),
				_ => throw new JsonException($"Unexpected action row inner component type: {type}.")
			};
			list.Add(component);
		}

		return list.ToArray();
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
		foreach (var c in value)
			JsonSerializer.Serialize(writer, c, c.GetType(), options);
		writer.WriteEndArray();
	}
}

using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages.Components;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.JsonConverters;

/// <summary>
/// JSON converter for arrays of <see cref="IInteractionComponent"/>. Deserializes each entry
/// based on its <c>type</c> discriminator (see <see cref="ComponentTypeMapping"/>); serializes
/// by runtime type so subclass-specific fields (Components V2) are preserved.
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
			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			if (!root.TryGetProperty("type", out var typeElement))
				throw new JsonException("Component missing 'type' field.");

			var componentType = (ComponentType)typeElement.GetInt32();
			components.Add(ComponentTypeMapping.Deserialize(componentType, root.GetRawText(), options));
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


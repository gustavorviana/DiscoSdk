using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.JsonConverters;

/// <summary>
/// JSON converter for <see cref="Color"/> that serializes/deserializes to/from integer.
/// </summary>
public class ColorConverter : JsonConverter<Color?>
{
	/// <inheritdoc />
	public override Color? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType == JsonTokenType.Number)
		{
			var value = reader.GetInt32();
			return new Color(value);
		}

		return null;
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, Color? value, JsonSerializerOptions options)
	{
		if (value.HasValue)
		{
			writer.WriteNumberValue(value.Value.Value);
		}
		else
		{
			writer.WriteNullValue();
		}
	}
}


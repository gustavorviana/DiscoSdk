using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.JsonConverters;

/// <summary>
/// JSON converter for <see cref="InteractionOption.Value"/> that converts JSON values to their appropriate .NET types.
/// </summary>
public class OptionValueConverter : JsonConverter<object?>
{
	/// <inheritdoc />
	public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		return reader.TokenType switch
		{
			JsonTokenType.String => reader.GetString(),
			JsonTokenType.True => true,
			JsonTokenType.False => false,
			JsonTokenType.Number => ReadNumber(ref reader),
			_ => null
		};
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}

		switch (value)
		{
			case string str:
				writer.WriteStringValue(str);
				break;
			case bool b:
				writer.WriteBooleanValue(b);
				break;
			case int i:
				writer.WriteNumberValue(i);
				break;
			case long l:
				writer.WriteNumberValue(l);
				break;
			case double d:
				writer.WriteNumberValue(d);
				break;
			case decimal dec:
				writer.WriteNumberValue(dec);
				break;
			case float f:
				writer.WriteNumberValue(f);
				break;
			default:
				// Fallback to string representation
				writer.WriteStringValue(value.ToString() ?? string.Empty);
				break;
		}
	}

	private static object ReadNumber(ref Utf8JsonReader reader)
	{
		// Try to read as int first (most common case)
		if (reader.TryGetInt32(out var intValue))
			return intValue;

		// Try long for larger integers
		if (reader.TryGetInt64(out var longValue))
			return longValue;

		// Try double for decimal numbers
		if (reader.TryGetDouble(out var doubleValue))
			return doubleValue;

		// Fallback: read as decimal
		return reader.GetDecimal();
	}
}


using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.JsonConverters;

/// <summary>
/// JSON converter that handles permission fields that can be either a string or an integer.
/// </summary>
public class PermissionStringConverter : JsonConverter<string>
{
	/// <inheritdoc />
	public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.String)
		{
			return reader.GetString() ?? string.Empty;
		}

		if (reader.TokenType == JsonTokenType.Number)
		{
			return reader.GetInt64().ToString();
		}

		return string.Empty;
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value);
	}
}

/// <summary>
/// JSON converter that handles permission fields that can be either a string or an integer, returning nullable string.
/// </summary>
public class PermissionStringNullableConverter : JsonConverter<string?>
{
	/// <inheritdoc />
	public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
		{
			return null;
		}

		if (reader.TokenType == JsonTokenType.String)
		{
			return reader.GetString();
		}

		if (reader.TokenType == JsonTokenType.Number)
		{
			return reader.GetInt64().ToString();
		}

		return null;
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNullValue();
		}
		else
		{
			writer.WriteStringValue(value);
		}
	}
}

/// <summary>
/// JSON converter that handles permission fields that can be either a string or an integer, returning integer.
/// </summary>
public class PermissionIntegerConverter : JsonConverter<int>
{
	/// <inheritdoc />
	public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Number)
		{
			return reader.GetInt32();
		}

		if (reader.TokenType == JsonTokenType.String)
		{
			var str = reader.GetString();
			if (int.TryParse(str, out var result))
			{
				return result;
			}
		}

		return 0;
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
	{
		writer.WriteNumberValue(value);
	}
}

/// <summary>
/// JSON converter that handles permission fields that can be either a string or an integer, returning nullable integer.
/// </summary>
public class PermissionIntegerNullableConverter : JsonConverter<int?>
{
	/// <inheritdoc />
	public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
		{
			return null;
		}

		if (reader.TokenType == JsonTokenType.Number)
		{
			return reader.GetInt32();
		}

		if (reader.TokenType == JsonTokenType.String)
		{
			var str = reader.GetString();
			if (int.TryParse(str, out var result))
			{
				return result;
			}
		}

		return null;
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
	{
		if (value.HasValue)
		{
			writer.WriteNumberValue(value.Value);
		}
		else
		{
			writer.WriteNullValue();
		}
	}
}


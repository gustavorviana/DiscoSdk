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

/// <summary>
/// JSON converter that serializes ApplicationCommandType enum as a number (non-nullable).
/// </summary>
public class ApplicationCommandTypeConverter : JsonConverter<ApplicationCommandType>
{
    /// <inheritdoc />
    public override ApplicationCommandType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            var value = reader.GetInt32();
            if (Enum.IsDefined(typeof(ApplicationCommandType), value))
            {
                return (ApplicationCommandType)value;
            }
        }

        return default;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, ApplicationCommandType value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue((int)value);
    }
}

/// <summary>
/// JSON converter that serializes ApplicationCommandType enum as a number (nullable).
/// </summary>
public class ApplicationCommandTypeNullableConverter : JsonConverter<ApplicationCommandType?>
{
    /// <inheritdoc />
    public override ApplicationCommandType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            var value = reader.GetInt32();
            if (Enum.IsDefined(typeof(ApplicationCommandType), value))
            {
                return (ApplicationCommandType)value;
            }
        }

        return null;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, ApplicationCommandType? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteNumberValue((int)value.Value);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}

/// <summary>
/// JSON converter that serializes ApplicationCommandOptionType enum as a number.
/// </summary>
public class ApplicationCommandOptionTypeConverter : JsonConverter<ApplicationCommandOptionType>
{
    /// <inheritdoc />
    public override ApplicationCommandOptionType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            var value = reader.GetInt32();
            if (Enum.IsDefined(typeof(ApplicationCommandOptionType), value))
            {
                return (ApplicationCommandOptionType)value;
            }
        }

        return default;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, ApplicationCommandOptionType value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue((int)value);
    }
}


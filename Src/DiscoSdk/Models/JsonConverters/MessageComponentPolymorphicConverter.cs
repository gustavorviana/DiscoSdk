using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages.Components;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.JsonConverters;

/// <summary>
/// JSON converter for a <em>single</em> <see cref="IMessageComponent"/> slot — used by
/// <see cref="SectionComponent.Accessory"/>. Serializes by runtime type; deserializes by the
/// <c>type</c> discriminator on the JSON object.
/// </summary>
public sealed class MessageComponentPolymorphicConverter : JsonConverter<IMessageComponent>
{
    /// <inheritdoc />
    public override IMessageComponent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("type", out var typeElement))
            throw new JsonException("Component missing 'type' field.");

        var componentType = (ComponentType)typeElement.GetInt32();
        return ComponentTypeMapping.Deserialize(componentType, root.GetRawText(), options) as IMessageComponent
            ?? throw new JsonException($"Component type {componentType} is not a valid IMessageComponent.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, IMessageComponent value, JsonSerializerOptions options)
    {
        // Serialize by RUNTIME type so V2 subclass-specific fields are included.
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}

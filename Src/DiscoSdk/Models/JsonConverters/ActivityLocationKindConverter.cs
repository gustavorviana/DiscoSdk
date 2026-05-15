using DiscoSdk.Models.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.JsonConverters;

/// <summary>
/// Maps <see cref="ActivityLocationKind"/> to Discord's wire format: the lowercase strings
/// <c>"gc"</c> (guild channel) and <c>"pc"</c> (private channel).
/// </summary>
internal sealed class ActivityLocationKindConverter : JsonConverter<ActivityLocationKind>
{
	public override ActivityLocationKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value = reader.GetString();
		return value switch
		{
			"gc" => ActivityLocationKind.GuildChannel,
			"pc" => ActivityLocationKind.PrivateChannel,
			_ => throw new JsonException($"Unknown ActivityLocationKind value '{value}'. Expected 'gc' or 'pc'."),
		};
	}

	public override void Write(Utf8JsonWriter writer, ActivityLocationKind value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value switch
		{
			ActivityLocationKind.GuildChannel => "gc",
			ActivityLocationKind.PrivateChannel => "pc",
			_ => throw new JsonException($"Cannot serialize ActivityLocationKind value {value}."),
		});
	}
}

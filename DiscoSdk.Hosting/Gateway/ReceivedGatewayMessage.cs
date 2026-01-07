using System.Text.Json;

namespace DiscoSdk.Hosting.Gateway;

/// <summary>
/// Represents a message received from the Discord Gateway.
/// </summary>
internal sealed class ReceivedGatewayMessage
{
    public OpCodes Opcode { get; private set; }
    public string? EventType { get; private set; }
    public long? SequenceNumber { get; private set; }
    public string? PayloadJson { get; private set; }

    private ReceivedGatewayMessage() { }

    public static ReceivedGatewayMessage Parse(string jsonMessage)
    {
        var message = new ReceivedGatewayMessage();
        using var document = JsonDocument.Parse(jsonMessage);

        var root = document.RootElement;

        message.Opcode = (OpCodes)root.GetProperty("op").GetInt32();

        if (root.TryGetProperty("t", out var t) && t.ValueKind != JsonValueKind.Null)
            message.EventType = t.GetString();

        if (root.TryGetProperty("s", out var s) && s.ValueKind != JsonValueKind.Null)
            message.SequenceNumber = s.GetInt64();

        if (root.TryGetProperty("d", out var d) && d.ValueKind != JsonValueKind.Null)
            message.PayloadJson = d.GetRawText();

        return message;
    }

    public bool HasPayload() => PayloadJson is not null;

    public bool IsSystem() =>
        Opcode is OpCodes.Heartbeat
        or OpCodes.Resume
        or OpCodes.Reconnect
        or OpCodes.InvalidSession
        or OpCodes.Hello
        or OpCodes.HeartbeatAck;

    public T? Deserialize<T>(JsonSerializerOptions? options = null)
    {
        if (string.IsNullOrEmpty(PayloadJson))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(PayloadJson, options);
        }
        catch (Exception)
        {
            return default;
        }
    }

    public JsonDocument ToJsonDocument()
    {
        if (string.IsNullOrEmpty(PayloadJson))
            return JsonDocument.Parse("{}");

        return JsonDocument.Parse(PayloadJson);
    }
}

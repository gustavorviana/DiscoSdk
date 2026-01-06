using System.Text.Json;

namespace DiscoSdk.Hosting.Gateway;

internal class ReceivedGatewayMessage : IDisposable
{
    private bool _disposed;
    private readonly JsonDocument _document;

    public OpCodes Opcode { get; }
    public string? EventType { get; }
    public long? SequenceNumber { get; }
    public JsonElement Payload { get; }

    public ReceivedGatewayMessage(string jsonMessage)
    {
        _document = JsonDocument.Parse(jsonMessage);
        var root = _document.RootElement;

        if (root.TryGetProperty("d", out var d))
            Payload = d;

        Opcode = (OpCodes)root.GetProperty("op").GetInt32();

        EventType = root.GetProperty("t").GetString();

        if (root.TryGetProperty("s", out var s) && s.ValueKind != JsonValueKind.Null)
            SequenceNumber = s.GetInt64();
    }

    public bool HasPayload()
    {
        return Payload.ValueKind != JsonValueKind.Null;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
            _document.Dispose();

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Determines whether the specified operation code represents a system-level operation.
    /// </summary>
    /// <param name="opCode">The operation code to evaluate.</param>
    /// <returns>true if the operation code corresponds to a system-level operation; otherwise, false.</returns>
    public bool IsSystem() =>
        Opcode is OpCodes.Heartbeat
        or OpCodes.Resume
        or OpCodes.Reconnect
        or OpCodes.InvalidSession
        or OpCodes.Hello
        or OpCodes.HeartbeatAck;
}

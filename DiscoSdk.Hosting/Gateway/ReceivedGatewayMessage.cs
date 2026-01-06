using System.Text.Json;

namespace DiscoSdk.Hosting.Gateway;

/// <summary>
/// Represents a message received from the Discord Gateway.
/// </summary>
internal class ReceivedGatewayMessage : IDisposable
{
    private bool _disposed;
    private readonly JsonDocument _document;

    /// <summary>
    /// Gets the operation code of the message.
    /// </summary>
    public OpCodes Opcode { get; }

    /// <summary>
    /// Gets the event type, if this is a dispatch message.
    /// </summary>
    public string? EventType { get; }

    /// <summary>
    /// Gets the sequence number of the message, if available.
    /// </summary>
    public long? SequenceNumber { get; }

    /// <summary>
    /// Gets the payload data of the message.
    /// </summary>
    public JsonElement Payload { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReceivedGatewayMessage"/> class.
    /// </summary>
    /// <param name="jsonMessage">The JSON string containing the gateway message.</param>
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

    /// <summary>
    /// Determines whether the message has a payload.
    /// </summary>
    /// <returns>true if the message has a payload; otherwise, false.</returns>
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

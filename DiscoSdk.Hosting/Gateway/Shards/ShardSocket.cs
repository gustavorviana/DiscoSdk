using DiscoSdk.Hosting.Gateway.Compression;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace DiscoSdk.Hosting.Gateway.Shards;

/// <summary>
/// Manages the WebSocket connection for a shard.
/// </summary>
internal class ShardSocket(GatewayDecompressFactory decompressFactory) : IDisposable
{
    private GatewayDecompress? _decompressor;
    private ClientWebSocket? _websocket;
    private bool _disposed;
    private long? _seq;

    /// <summary>
    /// Gets a value indicating whether the WebSocket is ready and open.
    /// </summary>
    public bool Ready => _websocket?.State == WebSocketState.Open;

    /// <summary>
    /// Connects to the Discord Gateway WebSocket endpoint.
    /// </summary>
    /// <param name="gatewayUri">The Gateway WebSocket URI.</param>
    /// <param name="token">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous connect operation.</returns>
    public  async Task ConnectAsync(Uri gatewayUri, CancellationToken token)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _seq = null;
        _websocket?.Dispose();
        _websocket = new ClientWebSocket();
        _websocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);

        _decompressor = decompressFactory.Create(_websocket);
        await _websocket.ConnectAsync(gatewayUri, token);
    }

    public Task ResumeAsync(string token, string sessionId, CancellationToken cancellationToken)
        => SendAsync(new(OpCodes.Resume, new
        {
            token,
            session_id = sessionId,
            seq = _seq
        }), cancellationToken);

    /// <summary>
    /// Reads a message from the WebSocket connection.
    /// </summary>
    /// <param name="token">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous read operation. The result contains the received message, or null if parsing fails.</returns>
    public async Task<ReceivedGatewayMessage?> ReadAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_websocket == null) return null;

        var json = await _decompressor!.ReceiveAsync(cancellationToken);

        try
        {
            var message =  ReceivedGatewayMessage.Parse(json);

            if (message.SequenceNumber != null)
                _seq = message.SequenceNumber;

            return message;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public Task SendHeartbeatAsync(CancellationToken cancellationToken)
        => SendAsync(OpCodes.Heartbeat, _seq, cancellationToken);

    public Task SendAsync(OpCodes codes, object? data, CancellationToken cancellationToken = default)
        => SendAsync(new(codes, data), cancellationToken);

    /// <summary>
    /// Sends a message to the Gateway WebSocket.
    /// </summary>
    /// <param name="payload">The message payload to send.</param>
    /// <param name="token">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public async Task SendAsync(SendGatewayMessage payload, CancellationToken token)
    {
        if (_websocket == null)
            return;

        var json = JsonSerializer.Serialize(new { op = payload.OpCode, d = payload.Data });
        var bytes = Encoding.UTF8.GetBytes(json);

        await _websocket.SendAsync(bytes, WebSocketMessageType.Text, true, token);
    }

    /// <summary>
    /// Closes the WebSocket connection gracefully.
    /// </summary>
    /// <returns>A task that represents the asynchronous close operation.</returns>
    public async Task Close()
    {
        if (_websocket == null)
            return;

        try
        {
            if (_websocket.State == WebSocketState.Open || _websocket.State == WebSocketState.CloseReceived)
                await _websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client shutdown", CancellationToken.None);
        }
        catch { }
        finally
        {
            try { _websocket?.Dispose(); } catch { }
            _websocket = null;
        }
    }

    #region IDisposable
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _decompressor?.Dispose();
            _websocket?.Dispose();
        }

        _disposed = true;
    }

    ~ShardSocket()
    {
        Dispose(disposing: false);
    }

    void IDisposable.Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
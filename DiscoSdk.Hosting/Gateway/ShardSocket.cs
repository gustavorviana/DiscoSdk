using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace DiscoSdk.Hosting.Gateway;

/// <summary>
/// Manages the WebSocket connection for a shard.
/// </summary>
internal class ShardSocket
{
    private ClientWebSocket? _ws;

    /// <summary>
    /// Gets a value indicating whether the WebSocket is ready and open.
    /// </summary>
    public bool Ready => _ws?.State == WebSocketState.Open;

    /// <summary>
    /// Connects to the Discord Gateway WebSocket endpoint.
    /// </summary>
    /// <param name="gatewayUri">The Gateway WebSocket URI.</param>
    /// <param name="token">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous connect operation.</returns>
    public async Task ConnectAsync(Uri gatewayUri, CancellationToken token)
    {
        _ws?.Dispose();
        _ws = new ClientWebSocket();
        _ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);

        await _ws.ConnectAsync(gatewayUri, token);
    }

    /// <summary>
    /// Reads a message from the WebSocket connection.
    /// </summary>
    /// <param name="token">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous read operation. The result contains the received message, or null if parsing fails.</returns>
    public async Task<ReceivedGatewayMessage?> ReadAsync(CancellationToken token)
    {
        if (_ws == null) return null;

        var sb = new StringBuilder();
        WebSocketReceiveResult r;

        var buffer = new byte[64 * 1024];

        do
        {
            r = await _ws.ReceiveAsync(buffer, token);

            if (r.MessageType == WebSocketMessageType.Close)
            {
                throw new WebSocketException("Gateway closed socket.");
            }

            sb.Append(Encoding.UTF8.GetString(buffer, 0, r.Count));
        }
        while (!r.EndOfMessage);

        try
        {
            return ReceivedGatewayMessage.Parse(sb.ToString());
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Sends a message to the Gateway WebSocket.
    /// </summary>
    /// <param name="payload">The message payload to send.</param>
    /// <param name="token">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public async Task SendAsync(SendGatewayMessage payload, CancellationToken token)
    {
        if (_ws == null)
            return;

        var json = JsonSerializer.Serialize(new { op = payload.OpCode, d = payload.Data });
        var bytes = Encoding.UTF8.GetBytes(json);

        await _ws.SendAsync(bytes, WebSocketMessageType.Text, true, token);
    }

    /// <summary>
    /// Closes the WebSocket connection gracefully.
    /// </summary>
    /// <returns>A task that represents the asynchronous close operation.</returns>
    public async Task Close()
    {
        if (_ws == null)
            return;

        try
        {
            if (_ws.State == WebSocketState.Open || _ws.State == WebSocketState.CloseReceived)
                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "closing", CancellationToken.None);
        }
        catch { }
        finally
        {
            try { _ws?.Dispose(); } catch { }
            _ws = null;
        }
    }
}
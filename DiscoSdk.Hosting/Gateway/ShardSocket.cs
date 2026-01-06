using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace DiscoSdk.Hosting.Gateway;

internal class ShardSocket
{
    private ClientWebSocket? _ws;

    public bool Ready => _ws?.State == WebSocketState.Open;

    public async Task ConnectAsync(Uri gatewayUri, CancellationToken token)
    {
        _ws?.Dispose();
        _ws = new ClientWebSocket();
        _ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);

        await _ws.ConnectAsync(gatewayUri, token);
    }

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
                Console.WriteLine("Closed");
                throw new WebSocketException("Gateway closed socket.");
            }

            sb.Append(Encoding.UTF8.GetString(buffer, 0, r.Count));
        }
        while (!r.EndOfMessage);

        try
        {
            return new ReceivedGatewayMessage(sb.ToString());
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task SendAsync(SendGatewayMessage payload, CancellationToken token)
    {
        if (_ws == null)
            return;

        var json = JsonSerializer.Serialize(new { op = payload.OpCode, d = payload.Data });
        var bytes = Encoding.UTF8.GetBytes(json);

        await _ws.SendAsync(bytes, WebSocketMessageType.Text, true, token);
    }

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
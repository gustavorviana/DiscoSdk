using System.Net.WebSockets;
using System.Text;

namespace DiscoSdk.Hosting.Gateway.Compression;

internal abstract class GatewayDecompress(WebSocket webSocket) : IDisposable
{
    private bool _disposed;

    protected WebSocket WebSocket => webSocket;
    protected virtual int BufferSize => 64 * 1024;

    public async Task<string> ReceiveAsync(CancellationToken cancellationToken)
        => Encoding.UTF8.GetString(await ReceiveBytesAsync(cancellationToken));

    internal abstract Task<byte[]> ReceiveBytesAsync(CancellationToken cancellationToken);

    protected void ThrowIfClosed(WebSocketReceiveResult result)
    {
        if (result.MessageType == WebSocketMessageType.Close)
            throw new DiscordSocketException(webSocket.CloseStatus ?? WebSocketCloseStatus.Empty, webSocket.CloseStatusDescription ?? "Gateway closed socket.");
    }

    #region IDisposed
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        _disposed = true;
    }

    ~GatewayDecompress()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
using System.Net.WebSockets;

namespace DiscoSdk.Hosting.Gateway.Compression;

internal class GatewayNoDecompress(WebSocket webSocket) : GatewayDecompress(webSocket)
{
    internal override async Task<byte[]> ReceiveBytesAsync(CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();
        var buffer = new byte[BufferSize];
        WebSocketReceiveResult result;

        do
        {
            result = await WebSocket.ReceiveAsync(buffer, cancellationToken);
            ThrowIfClosed(result);
            ms.Write(buffer, 0, result.Count);
        }
        while (!result.EndOfMessage);

        return ms.ToArray();
    }
}
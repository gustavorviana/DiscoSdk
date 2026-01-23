using System.Net.WebSockets;

namespace DiscoSdk.Hosting.Gateway.Compression;

internal class GatewayDecompressFactory(GatewayCompressMode mode)
{
    public GatewayDecompress Create(WebSocket webSocket)
    {
        if (mode == GatewayCompressMode.None)
            return new GatewayNoDecompress(webSocket);

        return new GatewayZlibDecompress(webSocket);
    }
}

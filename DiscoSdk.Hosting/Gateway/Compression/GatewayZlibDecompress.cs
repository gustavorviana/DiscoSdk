using DiscoSdk.Hosting.Gateway.Compression;
using System.IO.Compression;
using System.IO.Pipelines;
using System.Net.WebSockets;

internal sealed class GatewayZlibDecompress : GatewayDecompress
{
    internal static readonly byte[] ZlibSuffix = [0x00, 0x00, 0xFF, 0xFF];
    private readonly ZLibStream _inflater;
    private readonly Pipe _pipe = new();

    public GatewayZlibDecompress(WebSocket webSocket) : base(webSocket)
    {
        _inflater = new ZLibStream(_pipe.Reader.AsStream(), CompressionMode.Decompress, leaveOpen: true);
    }

    internal override async Task<byte[]> ReceiveBytesAsync(CancellationToken cancellationToken)
    {
        int pendingRead = await CopyWebSocketIncome(cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        using var resultStream = new MemoryStream();
        var outBuffer = new byte[64 * 1024];
        int read = 0;
        while (pendingRead > 0 && (read = await _inflater.ReadAsync(outBuffer, cancellationToken)) > 0)
        {
            resultStream.Write(outBuffer, 0, read);
            pendingRead -= read;
        }

        return resultStream.ToArray();
    }

    private async Task<int> CopyWebSocketIncome(CancellationToken cancellationToken)
    {
        var buffer = new byte[BufferSize];
        WebSocketReceiveResult result;
        int pendingRead = 0;

        do
        {
            result = await WebSocket.ReceiveAsync(buffer, cancellationToken);
            ThrowIfClosed(result);
            pendingRead += result.Count;
            await _pipe.Writer.WriteAsync(buffer.AsMemory(0, result.Count), cancellationToken);
        }
        while (!cancellationToken.IsCancellationRequested && !EndsWithSuffix(buffer, result.Count));

        await _pipe.Writer.FlushAsync(cancellationToken);

        return pendingRead;
    }

    private static bool EndsWithSuffix(byte[] buffer, int size)
    {
        if (buffer.Length < ZlibSuffix.Length)
            return false;

        var startIndex = size - ZlibSuffix.Length;

        for (int i = 0; i < ZlibSuffix.Length; i++)
            if (buffer[startIndex + i] != ZlibSuffix[i])
                return false;

        return true;
    }

    protected override void Dispose(bool disposing)
    {
        if (!disposing)
            return;

        _pipe.Writer.Complete();
        _inflater.Dispose();
        _pipe.Reader.Complete();
    }
}
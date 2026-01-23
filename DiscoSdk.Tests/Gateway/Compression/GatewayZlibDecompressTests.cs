using DiscoSdk.Hosting.Gateway;
using NSubstitute;
using System.IO.Compression;
using System.Net.WebSockets;
using System.Text;

namespace DiscoSdk.Tests.Gateway.Compression;

public class GatewayZlibDecompressTests
{
    [Fact]
    public async Task ReceiveBytesAsync_WithCompressedData_DecompressesCorrectly()
    {
        // Arrange
        var webSocket = Substitute.For<WebSocket>();
        var originalText = "Hello World";
        var compressedData = CompressData(Encoding.UTF8.GetBytes(originalText));

        // Adiciona o sufixo zlib (0x00, 0x00, 0xFF, 0xFF)
        var dataWithSuffix = new byte[compressedData.Length + 4];
        Array.Copy(compressedData, 0, dataWithSuffix, 0, compressedData.Length);
        dataWithSuffix[compressedData.Length] = 0x00;
        dataWithSuffix[compressedData.Length + 1] = 0x00;
        dataWithSuffix[compressedData.Length + 2] = 0xFF;
        dataWithSuffix[compressedData.Length + 3] = 0xFF;

        var callCount = 0;
        webSocket.ReceiveAsync(Arg.Any<ArraySegment<byte>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var buffer = callInfo.Arg<ArraySegment<byte>>();
                var isLast = callCount == 0;

                if (callCount == 0)
                {
                    var bytesToCopy = Math.Min(dataWithSuffix.Length, buffer.Count);
                    Array.Copy(dataWithSuffix, 0, buffer.Array!, buffer.Offset, bytesToCopy);
                    callCount++;
                    return Task.FromResult(new WebSocketReceiveResult(bytesToCopy, WebSocketMessageType.Binary, isLast));
                }

                return Task.FromResult(new WebSocketReceiveResult(0, WebSocketMessageType.Binary, true));
            });

        var decompress = new GatewayZlibDecompress(webSocket);

        // Act
        var result = await decompress.ReceiveBytesAsync(CancellationToken.None);

        // Assert
        var decompressedText = Encoding.UTF8.GetString(result);
        Assert.Equal(originalText, decompressedText);
    }

    [Fact]
    public async Task ReceiveBytesAsync_WithMultipleChunks_DecompressesCorrectly()
    {
        // Arrange
        var webSocket = Substitute.For<WebSocket>();
        var originalText = "Hello World from Discord Gateway";
        var compressedData = CompressData(Encoding.UTF8.GetBytes(originalText));

        // Divide em chunks
        var chunkSize = compressedData.Length / 2;
        var chunk1 = new byte[chunkSize];
        var chunk2 = new byte[compressedData.Length - chunkSize + 4]; // +4 para o sufixo

        Array.Copy(compressedData, 0, chunk1, 0, chunkSize);
        Array.Copy(compressedData, chunkSize, chunk2, 0, compressedData.Length - chunkSize);

        // Adiciona sufixo ao último chunk
        chunk2[compressedData.Length - chunkSize] = 0x00;
        chunk2[compressedData.Length - chunkSize + 1] = 0x00;
        chunk2[compressedData.Length - chunkSize + 2] = 0xFF;
        chunk2[compressedData.Length - chunkSize + 3] = 0xFF;

        var callCount = 0;
        webSocket.ReceiveAsync(Arg.Any<ArraySegment<byte>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var buffer = callInfo.Arg<ArraySegment<byte>>();

                if (callCount == 0)
                {
                    Array.Copy(chunk1, 0, buffer.Array!, buffer.Offset, chunk1.Length);
                    callCount++;
                    return Task.FromResult(new WebSocketReceiveResult(chunk1.Length, WebSocketMessageType.Binary, false));
                }

                Array.Copy(chunk2, 0, buffer.Array!, buffer.Offset, chunk2.Length);
                callCount++;
                return Task.FromResult(new WebSocketReceiveResult(chunk2.Length, WebSocketMessageType.Binary, true));
            });

        var decompress = new GatewayZlibDecompress(webSocket);

        // Act
        var result = await decompress.ReceiveBytesAsync(CancellationToken.None);

        // Assert
        var decompressedText = Encoding.UTF8.GetString(result);
        Assert.Equal(originalText, decompressedText);
    }

    [Fact]
    public async Task ReceiveBytesAsync_WhenWebSocketCloses_ThrowsDiscordSocketException()
    {
        // Arrange
        var webSocket = Substitute.For<WebSocket>();
        var closeResult = new WebSocketReceiveResult(0, WebSocketMessageType.Close, true, WebSocketCloseStatus.NormalClosure, "Connection closed");

        webSocket.CloseStatus.Returns(WebSocketCloseStatus.NormalClosure);
        webSocket.CloseStatusDescription.Returns("Connection closed");

        webSocket.ReceiveAsync(Arg.Any<ArraySegment<byte>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(closeResult));

        var decompress = new GatewayZlibDecompress(webSocket);

        // Act & Assert
        await Assert.ThrowsAsync<DiscordSocketException>(() => decompress.ReceiveBytesAsync(CancellationToken.None));
    }

    [Fact]
    public async Task ReceiveAsync_WithCompressedData_ReturnsCorrectString()
    {
        // Arrange
        var webSocket = Substitute.For<WebSocket>();
        var originalText = "Hello World";
        var compressedData = CompressData(Encoding.UTF8.GetBytes(originalText));

        var dataWithSuffix = new byte[compressedData.Length + 4];
        Array.Copy(compressedData, 0, dataWithSuffix, 0, compressedData.Length);
        dataWithSuffix[compressedData.Length] = 0x00;
        dataWithSuffix[compressedData.Length + 1] = 0x00;
        dataWithSuffix[compressedData.Length + 2] = 0xFF;
        dataWithSuffix[compressedData.Length + 3] = 0xFF;

        var callCount = 0;
        webSocket.ReceiveAsync(Arg.Any<ArraySegment<byte>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                if (callCount > 0)
                    return Task.FromResult(new WebSocketReceiveResult(0, WebSocketMessageType.Binary, true));

                var buffer = callInfo.Arg<ArraySegment<byte>>();
                var bytesToCopy = Math.Min(dataWithSuffix.Length, buffer.Count);
                Array.Copy(dataWithSuffix, 0, buffer.Array!, buffer.Offset, bytesToCopy);
                callCount++;
                return Task.FromResult(new WebSocketReceiveResult(bytesToCopy, WebSocketMessageType.Binary, true));
            });

        var decompress = new GatewayZlibDecompress(webSocket);

        // Act
        var result = await decompress.ReceiveAsync(CancellationToken.None);

        // Assert
        Assert.Equal(originalText, result);
    }

    [Fact]
    public void Dispose_DisposesCorrectly()
    {
        // Arrange
        var webSocket = Substitute.For<WebSocket>();
        var decompress = new GatewayZlibDecompress(webSocket);

        // Act
        decompress.Dispose();

        // Assert - não deve lançar exceção
        Assert.True(true);
    }

    [Fact]
    public async Task ReceiveBytesAsync_WithCancellationToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var webSocket = Substitute.For<WebSocket>();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        webSocket.ReceiveAsync(Arg.Any<ArraySegment<byte>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var ct = callInfo.Arg<CancellationToken>();
                ct.ThrowIfCancellationRequested();
                return Task.FromResult(new WebSocketReceiveResult(0, WebSocketMessageType.Binary, true));
            });

        var decompress = new GatewayZlibDecompress(webSocket);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => decompress.ReceiveBytesAsync(cts.Token));
    }

    private static byte[] CompressData(byte[] data)
    {
        using var output = new MemoryStream();
        using (var compressor = new ZLibStream(output, CompressionLevel.Fastest, leaveOpen: true))
        {
            compressor.Write(data, 0, data.Length);
        }
        return output.ToArray();
    }
}


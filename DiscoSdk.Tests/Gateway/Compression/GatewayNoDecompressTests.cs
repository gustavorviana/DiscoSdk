using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Hosting.Gateway.Compression;
using NSubstitute;
using System.Net.WebSockets;
using System.Text;

namespace DiscoSdk.Tests.Gateway.Compression;

public class GatewayNoDecompressTests
{
	[Fact]
	public async Task ReceiveBytesAsync_WithSingleMessage_ReturnsCorrectBytes()
	{
		// Arrange
		var webSocket = Substitute.For<WebSocket>();
		var expectedData = Encoding.UTF8.GetBytes("Hello World");
		var receiveResult = new WebSocketReceiveResult(expectedData.Length, WebSocketMessageType.Text, true);
		
		webSocket.ReceiveAsync(Arg.Any<ArraySegment<byte>>(), Arg.Any<CancellationToken>())
			.Returns(callInfo =>
			{
				var buffer = callInfo.Arg<ArraySegment<byte>>();
				Array.Copy(expectedData, 0, buffer.Array!, buffer.Offset, expectedData.Length);
				return Task.FromResult(receiveResult);
			});

		var decompress = new GatewayNoDecompress(webSocket);

		// Act
		var result = await decompress.ReceiveBytesAsync(CancellationToken.None);

		// Assert
		Assert.Equal(expectedData, result);
	}

	[Fact]
	public async Task ReceiveBytesAsync_WithMultipleChunks_CombinesChunks()
	{
		// Arrange
		var webSocket = Substitute.For<WebSocket>();
		var chunk1 = Encoding.UTF8.GetBytes("Hello ");
		var chunk2 = Encoding.UTF8.GetBytes("World");
		var expectedData = Encoding.UTF8.GetBytes("Hello World");
		
		var callCount = 0;
		webSocket.ReceiveAsync(Arg.Any<ArraySegment<byte>>(), Arg.Any<CancellationToken>())
			.Returns(callInfo =>
			{
				var buffer = callInfo.Arg<ArraySegment<byte>>();
				var result = callCount == 0
					? new WebSocketReceiveResult(chunk1.Length, WebSocketMessageType.Text, false)
					: new WebSocketReceiveResult(chunk2.Length, WebSocketMessageType.Text, true);
				
				if (callCount == 0)
				{
					Array.Copy(chunk1, 0, buffer.Array!, buffer.Offset, chunk1.Length);
				}
				else
				{
					Array.Copy(chunk2, 0, buffer.Array!, buffer.Offset, chunk2.Length);
				}
				
				callCount++;
				return Task.FromResult(result);
			});

		var decompress = new GatewayNoDecompress(webSocket);

		// Act
		var result = await decompress.ReceiveBytesAsync(CancellationToken.None);

		// Assert
		Assert.Equal(expectedData, result);
	}

	[Fact]
	public async Task ReceiveBytesAsync_WhenWebSocketCloses_ThrowsDiscordSocketException()
	{
		// Arrange
		var webSocket = Substitute.For<WebSocket>();
		var closeResult = new WebSocketReceiveResult(0, WebSocketMessageType.Close, true);
		closeResult = new WebSocketReceiveResult(0, WebSocketMessageType.Close, true, WebSocketCloseStatus.NormalClosure, "Connection closed");
		
		webSocket.CloseStatus.Returns(WebSocketCloseStatus.NormalClosure);
		webSocket.CloseStatusDescription.Returns("Connection closed");
		
		webSocket.ReceiveAsync(Arg.Any<ArraySegment<byte>>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(closeResult));

		var decompress = new GatewayNoDecompress(webSocket);

		// Act & Assert
		await Assert.ThrowsAsync<DiscordSocketException>(() => decompress.ReceiveBytesAsync(CancellationToken.None));
	}

	[Fact]
	public async Task ReceiveAsync_WithTextData_ReturnsCorrectString()
	{
		// Arrange
		var webSocket = Substitute.For<WebSocket>();
		var expectedText = "Hello World";
		var expectedData = Encoding.UTF8.GetBytes(expectedText);
		var receiveResult = new WebSocketReceiveResult(expectedData.Length, WebSocketMessageType.Text, true);
		
		webSocket.ReceiveAsync(Arg.Any<ArraySegment<byte>>(), Arg.Any<CancellationToken>())
			.Returns(callInfo =>
			{
				var buffer = callInfo.Arg<ArraySegment<byte>>();
				Array.Copy(expectedData, 0, buffer.Array!, buffer.Offset, expectedData.Length);
				return Task.FromResult(receiveResult);
			});

		var decompress = new GatewayNoDecompress(webSocket);

		// Act
		var result = await decompress.ReceiveAsync(CancellationToken.None);

		// Assert
		Assert.Equal(expectedText, result);
	}

	[Fact]
	public void Dispose_DisposesCorrectly()
	{
		// Arrange
		var webSocket = Substitute.For<WebSocket>();
		var decompress = new GatewayNoDecompress(webSocket);

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
				return Task.FromResult(new WebSocketReceiveResult(0, WebSocketMessageType.Text, true));
			});

		var decompress = new GatewayNoDecompress(webSocket);

		// Act & Assert
		await Assert.ThrowsAsync<OperationCanceledException>(() => decompress.ReceiveBytesAsync(cts.Token));
	}
}


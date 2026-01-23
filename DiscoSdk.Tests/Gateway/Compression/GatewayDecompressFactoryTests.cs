using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Hosting.Gateway.Compression;
using NSubstitute;
using System.Net.WebSockets;

namespace DiscoSdk.Tests.Gateway.Compression;

public class GatewayDecompressFactoryTests
{
	[Fact]
	public void Create_WithNoneMode_ReturnsGatewayNoDecompress()
	{
		// Arrange
		var factory = new GatewayDecompressFactory(GatewayCompressMode.None);
		var webSocket = Substitute.For<WebSocket>();

		// Act
		var result = factory.Create(webSocket);

		// Assert
		Assert.IsType<GatewayNoDecompress>(result);
	}

	[Fact]
	public void Create_WithZlibStreamMode_ReturnsGatewayZlibDecompress()
	{
		// Arrange
		var factory = new GatewayDecompressFactory(GatewayCompressMode.ZlibStream);
		var webSocket = Substitute.For<WebSocket>();

		// Act
		var result = factory.Create(webSocket);

		// Assert
		Assert.IsType<GatewayZlibDecompress>(result);
	}
}


using DiscoSdk.Hosting.Gateway.Compression;

namespace DiscoSdk.Hosting.Gateway.Shards;

/// <summary>
/// Builds <see cref="IGatewaySocket"/> instances. Production uses <see cref="DefaultGatewaySocketFactory"/>;
/// tests inject a factory that returns a controllable fake socket so they can drive the shard
/// without a real network connection.
/// </summary>
internal interface IGatewaySocketFactory
{
	IGatewaySocket Create();
}

/// <summary>
/// Default factory that produces a real <see cref="DefaultGatewaySocket"/> wrapping
/// <see cref="System.Net.WebSockets.ClientWebSocket"/> and the supplied decompression factory.
/// </summary>
internal sealed class DefaultGatewaySocketFactory(GatewayDecompressFactory decompressFactory) : IGatewaySocketFactory
{
	public IGatewaySocket Create() => new DefaultGatewaySocket(decompressFactory);
}

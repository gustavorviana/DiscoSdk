using DiscoSdk.Hosting.Gateway.Compression;

namespace DiscoSdk.Hosting.Gateway.Shards;

internal interface IShardPool : IShardEventListener
{
    IdentifyGate Gate { get; }
    DiscordGatewayUri GatewayUri { get; }
    CancellationToken CancellationToken { get; }
    GatewayDecompressFactory DecompressFactory { get; }
}
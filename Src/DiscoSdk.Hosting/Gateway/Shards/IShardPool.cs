namespace DiscoSdk.Hosting.Gateway.Shards;

internal interface IShardPool : IShardEventListener
{
    IdentifyGate Gate { get; }
    DiscordGatewayUri GatewayUri { get; }
    CancellationToken CancellationToken { get; }
    IGatewaySocketFactory SocketFactory { get; }
    TimeProvider TimeProvider { get; }
}
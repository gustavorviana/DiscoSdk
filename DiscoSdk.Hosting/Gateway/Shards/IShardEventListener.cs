using DiscoSdk.Hosting.Gateway.Payloads;

namespace DiscoSdk.Hosting.Gateway.Shards;

internal interface IShardEventListener
{
    /// <summary>
    /// Event raised when the shard receives a dispatch message from the Gateway.
    /// </summary>
    Task OnReceiveMessageAsync(Shard shard, ReceivedGatewayMessage message);

    /// <summary>
    /// Event raised when the shard receives a READY payload from the Gateway.
    /// </summary>
    Task OnReadyAsync(Shard shard, ReadyPayload payload);

    /// <summary>
    /// Event raised when the shard successfully resumes a connection.
    /// </summary>
    Task OnResumeAsync(Shard shard);

    /// <summary>
    /// Event raised when the shard loses connection to the Gateway.
    /// </summary>
    Task OnConnectionLostAsync(Shard shard, Exception exception);
}

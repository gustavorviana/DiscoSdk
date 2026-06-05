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

    /// <summary>
    /// Raised once per retry attempt during the exponential backoff. Observational —
    /// the listener cannot vote on whether to proceed or alter the delay.
    /// </summary>
    Task OnReconnectingAsync(Shard shard, int attempt, TimeSpan delay, bool isResume);

    /// <summary>
    /// Raised when the shard encountered an exception the recovery path cannot handle — the run
    /// loop has already exited. The listener is expected to mark the client as terminally failed
    /// so callers awaiting shutdown observe the cause instead of a clean return.
    /// </summary>
    Task OnFatalAsync(Shard shard, Exception exception);

    void OnUnhandledError(Exception exception);
}

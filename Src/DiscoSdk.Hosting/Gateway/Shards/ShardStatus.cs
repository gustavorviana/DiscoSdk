namespace DiscoSdk.Hosting.Gateway.Shards;

/// <summary>
/// Represents the connection status of a shard.
/// </summary>
public enum ShardStatus
{
    /// <summary>
    /// The shard has lost connection to the Gateway.
    /// </summary>
    ConnectionLost,

    /// <summary>
    /// The shard is waiting for the HELLO message from the Gateway.
    /// </summary>
    PendingHello,

    /// <summary>
    /// The shard is waiting for acknowledgment after identifying.
    /// </summary>
    PendingAck,

    /// <summary>
    /// The shard is ready and fully connected to the Gateway.
    /// </summary>
    Ready
}

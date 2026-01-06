namespace DiscoSdk.Hosting.Gateway;

public enum ShardStatus
{
    ConnectionLost,
    PendingHello,
    PendingAck,
    Ready
}

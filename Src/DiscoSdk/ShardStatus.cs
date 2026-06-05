namespace DiscoSdk;

/// <summary>
/// Public-facing lifecycle state of a shard's gateway connection. Surfaced via
/// <see cref="IShard.Status"/> so bot authors can render dashboards, drive health probes, or
/// gate behaviour while a shard is mid-handshake.
/// </summary>
public enum ShardStatus
{
    /// <summary>Initial state, or transport is closed and the SDK is not currently retrying
    /// (e.g. <c>AutoReconnect=false</c>, or <c>StopAsync</c> was called).</summary>
    Disconnected,

    /// <summary>WebSocket dial completed; waiting for the gateway's HELLO frame.</summary>
    Connecting,

    /// <summary>HELLO received; IDENTIFY or RESUME has been sent; waiting for READY/RESUMED.</summary>
    Identifying,

    /// <summary>READY/RESUMED dispatch received. Heartbeating and serving events.</summary>
    Ready,

    /// <summary>Connection dropped; the SDK is in the exponential backoff retry loop. Each
    /// <see cref="Events.GatewayReconnectingEventArgs"/> fires while in this state.</summary>
    Reconnecting,

    /// <summary>Hit a fatal close code (4004 / 4010–4014) or an unrecoverable internal error.
    /// The shard's run loop has exited; only a client-wide
    /// <see cref="IDiscordClient.ReconnectAsync"/> or full <c>StopAsync</c>/<c>StartAsync</c>
    /// can recover.</summary>
    Fatal,
}

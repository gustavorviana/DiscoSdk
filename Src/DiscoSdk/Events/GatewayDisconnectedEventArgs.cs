namespace DiscoSdk.Events;

/// <summary>
/// Raised when a shard's gateway connection is lost. The args describe what happened and what the
/// SDK is about to do — the handler is observational, it does not vote on the decision.
/// </summary>
/// <remarks>
/// <para>
/// The SDK manages reconnect automatically when <c>DiscordClientConfig.AutoReconnect</c> is
/// <c>true</c> (default), retrying with exponential backoff. To force a client-wide
/// reconciliation outside the auto path, call <see cref="IDiscordClient.ReconnectAsync"/>.
/// </para>
/// <list type="bullet">
///   <item><b>Auto-reconnect on (default):</b> <see cref="WillReconnect"/> is <c>true</c>; the SDK
///   waits the configured backoff and reconnects. The handler should log/alert.</item>
///   <item><b>Auto-reconnect off:</b> <see cref="WillReconnect"/> is <c>false</c>; the shard stays
///   disconnected. The bot must call <see cref="IDiscordClient.StopAsync"/> and rebuild, or
///   <see cref="IDiscordClient.ReconnectAsync"/> to bring every shard back.</item>
/// </list>
/// </remarks>
public sealed class GatewayDisconnectedEventArgs : EventArgs
{
    public GatewayDisconnectedEventArgs(IShard shard, Exception exception, bool willReconnect)
    {
        Shard = shard;
        Exception = exception;
        WillReconnect = willReconnect;
    }

    /// <summary>The shard that lost its connection.</summary>
    public IShard Shard { get; }

    /// <summary>The exception that caused the disconnect (transport, missed heartbeat, etc).</summary>
    public Exception Exception { get; }

    /// <summary>
    /// Whether the SDK will attempt to reconnect automatically. Driven by
    /// <c>DiscordClientConfig.AutoReconnect</c> AND whether the underlying exception is
    /// recoverable (fatal close codes set this to <c>false</c> even with AutoReconnect on).
    /// </summary>
    public bool WillReconnect { get; }
}

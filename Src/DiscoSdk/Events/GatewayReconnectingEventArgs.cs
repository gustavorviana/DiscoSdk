namespace DiscoSdk.Events;

/// <summary>
/// Raised once per retry attempt while a shard is reconnecting after a drop. Observational —
/// the args are read-only; bot authors who need to alter recovery behaviour have three escape
/// hatches: <c>DiscordClientConfig.AutoReconnect</c> (global kill switch), <c>ReconnectDelay</c>
/// (base of the exponential backoff), and <see cref="IDiscordClient.ReconnectAsync"/> (force an
/// immediate full-pool reconciliation).
/// </summary>
public sealed class GatewayReconnectingEventArgs : EventArgs
{
    public GatewayReconnectingEventArgs(IShard shard, int attempt, TimeSpan delay, bool isResume)
    {
        Shard = shard;
        Attempt = attempt;
        Delay = delay;
        IsResume = isResume;
    }

    /// <summary>The shard currently retrying.</summary>
    public IShard Shard { get; }

    /// <summary>1-indexed attempt number within the current outage. Resets to 1 once the shard
    /// reconnects successfully.</summary>
    public int Attempt { get; }

    /// <summary>How long the SDK will wait before sending the next CONNECT — already includes
    /// the exponential growth and the configured 900s cap.</summary>
    public TimeSpan Delay { get; }

    /// <summary><c>true</c> when this attempt will RESUME against the cached session id;
    /// <c>false</c> when it will open a fresh IDENTIFY.</summary>
    public bool IsResume { get; }
}

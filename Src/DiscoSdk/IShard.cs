namespace DiscoSdk;

/// <summary>
/// A single Discord gateway shard. Exposes the minimal observation surface the bot author needs
/// to inspect a shard's connection state. Per-message send and recovery control remain internal —
/// the SDK manages the connection lifecycle; use <see cref="IDiscordClient.ReconnectAsync"/> for
/// a forced reconciliation of every shard.
/// </summary>
public interface IShard
{
    /// <summary>
    /// Zero-based shard id within the bot's total shard count.
    /// </summary>
    int Id { get; }

    /// <summary>
    /// <c>true</c> when this shard has completed the <c>READY</c> handshake and is serving events.
    /// Flips to <c>false</c> at the start of any disconnect / reconnect cycle and back to
    /// <c>true</c> once the new session (or resume) lands.
    /// </summary>
    bool IsReady { get; }

    /// <summary>
    /// Granular lifecycle state — useful when <see cref="IsReady"/>'s boolean is not enough
    /// (dashboards, health probes, telemetry that distinguishes "reconnecting" from "fatal").
    /// </summary>
    ShardStatus Status { get; }
}

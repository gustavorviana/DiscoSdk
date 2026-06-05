namespace DiscoSdk.Hosting.Gateway;

/// <summary>
/// Canonical strings used as WebSocket close reasons, exception messages, and log details across
/// the gateway code paths. Centralised so a wording or wording-format change is a single edit and
/// so test assertions can pin against a stable constant rather than a literal.
/// </summary>
internal static class GatewayMessages
{
    /// <summary>Reason passed with a graceful (code 1000) close from our side.</summary>
    public const string ClientShutdown = "Client shutdown";

    /// <summary>Reason passed with the non-1000 close used after a missed HEARTBEAT_ACK or any
    /// other transport-layer fault that the receive loop did not raise directly.</summary>
    public const string TransportFault = "Transport fault";

    /// <summary>Exception message the fake socket reports when a pending ReadAsync is interrupted
    /// by a close. Mirrors the kind of message a disposed <c>ClientWebSocket</c> surfaces.</summary>
    public const string SocketClosed = "Socket closed.";

    /// <summary>Fallback message when the decompressor encounters a close frame whose
    /// <c>CloseStatusDescription</c> is absent.</summary>
    public const string GatewayClosedSocket = "Gateway closed socket.";

    /// <summary>Message attached to the synthetic <c>WebSocketException</c> raised when the
    /// gateway accepts the WebSocket dial but never sends HELLO.</summary>
    public const string HelloTimeout = "Gateway did not send HELLO within the configured timeout.";

    /// <summary>Message wrapped in <c>DiscordFatalException</c> when a shard exits the run loop
    /// because of an unrecoverable exception.</summary>
    public const string DiscordClientTerminated = "Discord client terminated due to an unrecoverable error.";

    /// <summary>Format for the missed-heartbeat exception. The single argument is the shard id.</summary>
    public static string MissedHeartbeatAck(int shardId) => $"Shard {shardId} missed HEARTBEAT_ACK.";
}

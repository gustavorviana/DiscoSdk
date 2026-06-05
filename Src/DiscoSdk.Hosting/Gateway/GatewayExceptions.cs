using System.Net.WebSockets;

namespace DiscoSdk.Hosting.Gateway;

/// <summary>
/// Single source of truth for classifying exceptions raised by the gateway transport. Used by
/// both <see cref="Shards.Shard"/>'s catch path (to decide between reconnect and fatal) and
/// <see cref="DiscordClient"/>'s <c>OnConnectionLostAsync</c> (to set
/// <c>GatewayDisconnectedEventArgs.WillReconnect</c> truthfully). Keep the two in sync by routing
/// every check through here.
/// </summary>
internal static class GatewayExceptions
{
    /// <summary>
    /// Close codes the gateway emits that the recovery path cannot resolve by reconnecting:
    /// authentication failure, invalid shard, sharding required, invalid API version, invalid
    /// intents, disallowed intents. Reconnecting on any of these would loop forever.
    /// </summary>
    public static bool IsFatalCloseCode(int code) => code switch
    {
        4004 => true, // Authentication failed
        4010 => true, // Invalid shard
        4011 => true, // Sharding required
        4012 => true, // Invalid API version
        4013 => true, // Invalid intents
        4014 => true, // Disallowed intents
        _ => false
    };

    /// <summary>
    /// <c>true</c> when the exception originated from the WebSocket / Discord transport layer
    /// AND the close code (when present) is one the recovery path can act on. <c>false</c> for
    /// any other exception type or for a fatal close code — those route through
    /// <see cref="Shards.IShardEventListener.OnFatalAsync"/>.
    /// </summary>
    public static bool IsRecoverableTransport(Exception exception) => exception switch
    {
        DiscordSocketException dse => !IsFatalCloseCode(dse.CloseCode),
        WebSocketException => true,
        _ => false
    };
}

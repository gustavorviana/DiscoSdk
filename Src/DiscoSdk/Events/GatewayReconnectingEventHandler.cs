namespace DiscoSdk.Events;

/// <summary>
/// Handler signature for <see cref="IDiscordClient.GatewayReconnecting"/>. Fires once per retry
/// attempt while the shard is climbing the exponential backoff; the client awaits each handler
/// in subscription order before proceeding to the configured delay.
/// </summary>
public delegate Task GatewayReconnectingEventHandler(GatewayReconnectingEventArgs args);

namespace DiscoSdk.Events;

/// <summary>
/// Handler signature for <see cref="IDiscordClient.GatewayDisconnected"/>. Returns a <see cref="Task"/>
/// so handlers can perform I/O (alerts, persistence, observability); the client awaits each
/// handler in subscription order before dispatching the next shard event.
/// </summary>
public delegate Task GatewayDisconnectedEventHandler(GatewayDisconnectedEventArgs args);

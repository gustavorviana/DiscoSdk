using DiscoSdk.Hosting.Gateway.Payloads;

namespace DiscoSdk.Hosting.Gateway.Shards;

/// <summary>
/// Abstracts the Discord Gateway WebSocket connection so the <see cref="Shard"/> can be tested
/// without a real <see cref="System.Net.WebSockets.ClientWebSocket"/>. Compression is owned by the
/// implementation — <see cref="ReadAsync"/> returns already-decompressed messages.
/// </summary>
internal interface IGatewaySocket : IDisposable
{
	/// <summary>Whether the underlying connection is currently open.</summary>
	bool Ready { get; }

	/// <summary>Opens a new gateway connection to <paramref name="gatewayUri"/>.</summary>
	Task ConnectAsync(Uri gatewayUri, CancellationToken token);

	/// <summary>
	/// Reads and decompresses the next message from the gateway. Returns <c>null</c> when parsing fails.
	/// </summary>
	Task<ReceivedGatewayMessage?> ReadAsync(CancellationToken cancellationToken);

	/// <summary>Sends a pre-built gateway payload.</summary>
	Task SendAsync(SendGatewayMessage payload, CancellationToken token);

	/// <summary>Builds and sends an opcode payload.</summary>
	Task SendAsync(OpCodes opcode, object? data, CancellationToken cancellationToken = default);

	/// <summary>Sends the current sequence number as a HEARTBEAT.</summary>
	Task SendHeartbeatAsync(CancellationToken cancellationToken);

	/// <summary>Sends a RESUME payload with the supplied token and session id.</summary>
	Task ResumeAsync(string token, string sessionId, CancellationToken cancellationToken);

	/// <summary>Closes the connection gracefully.</summary>
	Task Close();
}

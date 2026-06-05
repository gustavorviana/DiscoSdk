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

	/// <summary>Closes the connection gracefully (code 1000).</summary>
	Task Close();

	/// <summary>
	/// Closes the connection with a custom WebSocket close code. Per Discord spec, a missed
	/// HEARTBEAT_ACK must terminate with a non-1000 close so the server keeps the session
	/// resumable rather than treating it as a clean disconnect.
	/// </summary>
	Task CloseAsync(int closeCode, string reason);

	/// <summary>
	/// Drops the current session's sequence number so the next <see cref="SendHeartbeatAsync"/>
	/// and <see cref="ResumeAsync"/> behave as a fresh session. Called by the shard before a
	/// non-resume reconnect; resume reconnects must NOT call this — Discord rejects a RESUME
	/// payload whose <c>seq</c> does not match the last event it sent.
	/// </summary>
	void ResetSequence();
}

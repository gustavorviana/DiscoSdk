using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Hosting.Gateway.Shards;
using System.Net.WebSockets;
using System.Threading.Channels;

namespace DiscoSdk.Hosting.Tests.Gateway.Common;

/// <summary>
/// Test double for <see cref="IGatewaySocket"/>. Lets a test push inbound frames at the shard via
/// <see cref="EnqueueInbound"/> and capture every outbound payload the shard sends in
/// <see cref="SentFrames"/>. No real network, no compression — frames are passed through as-is.
/// </summary>
internal sealed class FakeGatewaySocket : IGatewaySocket
{
	private static readonly UnboundedChannelOptions InboxOptions = new()
	{
		SingleReader = true,
		SingleWriter = false,
	};

	// Reassigned by ConnectAsync so a reconnect after Close gets a fresh inbox — mirrors prod, where
	// a new ClientWebSocket is allocated per connect.
	private Channel<ReceivedGatewayMessage> _inbox = Channel.CreateUnbounded<ReceivedGatewayMessage>(InboxOptions);
	private readonly List<SendGatewayMessage> _sentFrames = [];
	private readonly object _sync = new();
	private long? _seq;
	private int _pendingInbound;

	public bool Ready { get; private set; }
	public Uri? ConnectedTo { get; private set; }
	public bool Closed { get; private set; }
	public int ConnectCount { get; private set; }
	private readonly Queue<Exception> _pendingConnectFaults = new();

	/// <summary>Snapshot of every frame the shard has sent so far.</summary>
	public IReadOnlyList<SendGatewayMessage> SentFrames
	{
		get { lock (_sync) return _sentFrames.ToArray(); }
	}

	/// <summary>Pushes an inbound frame so the shard's receive loop picks it up.</summary>
	public ValueTask EnqueueInbound(ReceivedGatewayMessage frame)
	{
		Interlocked.Increment(ref _pendingInbound);
		return _inbox.Writer.WriteAsync(frame);
	}

	/// <summary>
	/// Yields until the inbox is empty — i.e., the shard's receive loop has called
	/// <see cref="ReadAsync"/> for every enqueued frame. Tests must call this between an
	/// <see cref="EnqueueInbound"/> that drives shard state (e.g. HEARTBEAT_ACK) and any subsequent
	/// virtual-time advance, otherwise the receive loop may not have applied the frame yet.
	/// </summary>
	public async Task WaitForInboxDrainedAsync(int timeoutMs = 1000)
	{
		var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
		while (DateTime.UtcNow < deadline)
		{
			if (Volatile.Read(ref _pendingInbound) == 0)
			{
				// One more yield so the consumer's continuation past ReadAsync actually runs.
				await Task.Yield();
				return;
			}
			await Task.Delay(1);
		}
		throw new TimeoutException("Inbox did not drain within timeout.");
	}

	public Task ConnectAsync(Uri gatewayUri, CancellationToken token)
	{
		ConnectCount++;

		// Test seam: simulate transient connect failures (server down, DNS flake) before letting
		// the connection succeed. Each enqueued fault fires for a single ConnectAsync attempt.
		if (_pendingConnectFaults.TryDequeue(out var fault))
			throw fault;

		ConnectedTo = gatewayUri;
		Ready = true;
		Closed = false;
		// A reconnect after Close needs a usable inbox; the previous one was completed.
		Volatile.Write(ref _pendingInbound, 0);
		_inbox = Channel.CreateUnbounded<ReceivedGatewayMessage>(InboxOptions);
		return Task.CompletedTask;
	}

	/// <summary>Enqueue an exception to throw on the next <see cref="ConnectAsync"/> call.</summary>
	public void QueueConnectFault(Exception exception) => _pendingConnectFaults.Enqueue(exception);

	public void ResetSequence() => _seq = null;

	public async Task<ReceivedGatewayMessage?> ReadAsync(CancellationToken cancellationToken)
	{
		try
		{
			var message = await _inbox.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
			Interlocked.Decrement(ref _pendingInbound);
			if (message.SequenceNumber.HasValue)
				_seq = message.SequenceNumber.Value;
			return message;
		}
		catch (ChannelClosedException ex) when (ex.InnerException is not null)
		{
			// Mirror prod: a disposed ClientWebSocket throws the transport exception directly,
			// not the Channel abstraction's wrapper. Unwrap so the shard's catch sees the same type.
			throw ex.InnerException;
		}
	}

	public Task SendAsync(SendGatewayMessage payload, CancellationToken token)
	{
		lock (_sync)
			_sentFrames.Add(payload);
		return Task.CompletedTask;
	}

	public Task SendAsync(OpCodes opcode, object? data, CancellationToken cancellationToken = default)
		=> SendAsync(new SendGatewayMessage(opcode, data), cancellationToken);

	public Task SendHeartbeatAsync(CancellationToken cancellationToken)
		=> SendAsync(OpCodes.Heartbeat, _seq, cancellationToken);

	public Task ResumeAsync(string token, string sessionId, CancellationToken cancellationToken)
		=> SendAsync(new SendGatewayMessage(OpCodes.Resume, new
		{
			token,
			session_id = sessionId,
			seq = _seq,
		}), cancellationToken);

	public Task Close() => CloseAsync(1000, GatewayMessages.ClientShutdown);

	public int? LastCloseCode { get; private set; }
	public string? LastCloseReason { get; private set; }

	public Task CloseAsync(int closeCode, string reason)
	{
		Closed = true;
		Ready = false;
		LastCloseCode = closeCode;
		LastCloseReason = reason;
		// Mirrors prod: disposing a ClientWebSocket while a ReceiveAsync is pending makes the
		// pending read throw. Completing the channel with an exception delivers the same signal.
		_inbox.Writer.TryComplete(new WebSocketException(GatewayMessages.SocketClosed));
		return Task.CompletedTask;
	}

	/// <summary>
	/// Test seam: force the next <see cref="ReadAsync"/> to throw <paramref name="exception"/>.
	/// Used to drive the shard's catch path with non-transport exceptions so fatal flow can be
	/// exercised end-to-end.
	/// </summary>
	public void InjectReadFault(Exception exception)
	{
		_inbox.Writer.TryComplete(exception);
	}

	public void Dispose() { }
}

/// <summary>
/// Factory that always returns the same supplied <see cref="FakeGatewaySocket"/>. Used in tests
/// where there's only one shard.
/// </summary>
internal sealed class FakeGatewaySocketFactory(FakeGatewaySocket socket) : IGatewaySocketFactory
{
	public IGatewaySocket Create() => socket;
}

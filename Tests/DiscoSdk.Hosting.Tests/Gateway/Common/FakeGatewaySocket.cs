using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Hosting.Gateway.Shards;
using System.Threading.Channels;

namespace DiscoSdk.Hosting.Tests.Gateway.Common;

/// <summary>
/// Test double for <see cref="IGatewaySocket"/>. Lets a test push inbound frames at the shard via
/// <see cref="EnqueueInbound"/> and capture every outbound payload the shard sends in
/// <see cref="SentFrames"/>. No real network, no compression — frames are passed through as-is.
/// </summary>
internal sealed class FakeGatewaySocket : IGatewaySocket
{
	private readonly Channel<ReceivedGatewayMessage> _inbox = Channel.CreateUnbounded<ReceivedGatewayMessage>(new UnboundedChannelOptions
	{
		SingleReader = true,
		SingleWriter = false,
	});
	private readonly List<SendGatewayMessage> _sentFrames = [];
	private readonly object _sync = new();
	private long? _seq;
	private int _pendingInbound;

	public bool Ready { get; private set; }
	public Uri? ConnectedTo { get; private set; }
	public bool Closed { get; private set; }
	public int ConnectCount { get; private set; }

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
		ConnectedTo = gatewayUri;
		ConnectCount++;
		Ready = true;
		Closed = false;
		return Task.CompletedTask;
	}

	public async Task<ReceivedGatewayMessage?> ReadAsync(CancellationToken cancellationToken)
	{
		var message = await _inbox.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
		Interlocked.Decrement(ref _pendingInbound);
		if (message.SequenceNumber.HasValue)
			_seq = message.SequenceNumber.Value;
		return message;
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

	public Task Close()
	{
		Closed = true;
		Ready = false;
		return Task.CompletedTask;
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

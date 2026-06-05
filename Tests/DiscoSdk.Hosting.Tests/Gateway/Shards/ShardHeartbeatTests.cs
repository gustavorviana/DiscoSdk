using DiscoSdk.Hosting;
using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Hosting.Gateway.Shards;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using Microsoft.Extensions.Time.Testing;
using System.Net.WebSockets;

namespace DiscoSdk.Hosting.Tests.Gateway.Shards;

/// <summary>
/// Verifies the heartbeat loop fires on the cadence Discord supplies in HELLO. Uses
/// <see cref="FakeTimeProvider"/> to advance virtual time and a <see cref="FakeGatewaySocket"/>
/// to capture the sent HEARTBEAT frames.
/// </summary>
public class ShardHeartbeatTests
{
	private readonly FakeGatewaySocket _socket = new();
	private readonly FakeTimeProvider _time = new();
	private readonly FakeShardPool _pool;
	private readonly DiscoSdk.Hosting.Gateway.Shards.Shard _shard;

	public ShardHeartbeatTests()
	{
		_pool = new FakeShardPool(_socket, _time);
		_shard = new DiscoSdk.Hosting.Gateway.Shards.Shard(0, new DiscordClientConfig
		{
			Token = "test-token",
			Intents = DiscordIntent.Guilds,
			ReconnectDelay = TimeSpan.FromSeconds(5),
			HeartbeatJitter = 0.0,
			ReconnectBackoffJitter = 0.0,
		}, _pool);
	}

	[Fact]
	public async Task OnInboundHeartbeatRequest_SendsHeartbeatImmediatelyAsync()
	{
		await _shard.StartAsync();
		await _socket.EnqueueInbound(TestFrames.Hello(heartbeatIntervalMs: 5000));
		await _socket.EnqueueInbound(TestFrames.Ready());
		await WaitFor(() => _shard.Status == ShardStatus.Ready);
		await WaitForOpcodeCount(OpCodes.Heartbeat, 1);

		// Server-side OP 1 (Heartbeat) — Discord asks for an out-of-band heartbeat now.
		await _socket.EnqueueInbound(TestFrames.HeartbeatRequest());

		// Out-of-band heartbeat fires without waiting for the next interval.
		await WaitForOpcodeCount(OpCodes.Heartbeat, 2);
	}

	[Fact]
	public async Task OnHello_SendsImmediateInitialHeartbeatAsync()
	{
		await _shard.StartAsync();
		await _socket.EnqueueInbound(TestFrames.Hello(heartbeatIntervalMs: 5000));

		// The heartbeat loop sends one heartbeat immediately on Hello, before the first interval delay.
		await WaitForOpcodeCount(OpCodes.Heartbeat, 1);
		Assert.Single(_socket.SentFrames.Where(f => f.OpCode == OpCodes.Heartbeat));
	}

	[Fact]
	public async Task HeartbeatLoop_FiresOncePerIntervalAfterAckAsync()
	{
		await _shard.StartAsync();
		await _socket.EnqueueInbound(TestFrames.Hello(heartbeatIntervalMs: 5000));

		// Initial heartbeat fires immediately; ack it so the loop arms the next one.
		await WaitForOpcodeCount(OpCodes.Heartbeat, 1);
		await _socket.EnqueueInbound(TestFrames.HeartbeatAck());
		// Wait for the shard's receive loop to consume the ACK before advancing time — otherwise
		// the heartbeat task can wake before _heartbeatAck flips and throws MissingAck.
		await _socket.WaitForInboxDrainedAsync();

		// Advance virtual time past the interval — second heartbeat should fire.
		_time.Advance(TimeSpan.FromSeconds(5));
		await WaitForOpcodeCount(OpCodes.Heartbeat, 2);
	}

	[Fact]
	public async Task MissedHeartbeatAck_PublishesFaultAndReconnectsAsync()
	{
		await _shard.StartAsync();
		await _socket.EnqueueInbound(TestFrames.Hello(heartbeatIntervalMs: 5000));
		await _socket.EnqueueInbound(TestFrames.Ready(sessionId: "sess-hb", resumeGatewayUrl: "wss://resume.test/"));
		await WaitFor(() => _shard.Status == ShardStatus.Ready);

		// Initial heartbeat sent; deliberately do NOT enqueue an ACK so the next tick raises a
		// missed-ack fault.
		await WaitForOpcodeCount(OpCodes.Heartbeat, 1);

		// Advance past the heartbeat interval — the missed-ack path publishes the transport fault
		// and the receive loop's catch logs it on the pool.
		_time.Advance(TimeSpan.FromSeconds(5));
		await WaitFor(() => _pool.ConnectionLostEvents.Count > 0);
		Assert.IsType<WebSocketException>(_pool.ConnectionLostEvents[0]);

		// Advance past ReconnectDelay so the catch path completes ConnectAsync.
		_time.Advance(TimeSpan.FromSeconds(5));
		await WaitFor(() => _socket.ConnectCount >= 2);

		// Opportunistic resume: we still hold a valid session id and resume URL from the READY,
		// so the recovery path targets the resume URL rather than wiping the session.
		Assert.Equal(new Uri("wss://resume.test/"), _socket.ConnectedTo);
	}

	private static async Task WaitFor(Func<bool> condition, int timeoutMs = 2000)
	{
		var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
		while (DateTime.UtcNow < deadline)
		{
			if (condition()) return;
			await Task.Delay(5);
		}
		throw new TimeoutException("Condition not met within timeout.");
	}

	private async Task WaitForOpcodeCount(OpCodes op, int expected, int timeoutMs = 1000)
	{
		var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
		while (DateTime.UtcNow < deadline)
		{
			if (_socket.SentFrames.Count(f => f.OpCode == op) >= expected)
				return;
			await Task.Delay(5);
		}
		throw new TimeoutException($"Expected {expected} {op} frame(s); got {_socket.SentFrames.Count(f => f.OpCode == op)}.");
	}
}

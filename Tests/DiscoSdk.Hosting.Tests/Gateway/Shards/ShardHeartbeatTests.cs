using DiscoSdk.Hosting;
using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using Microsoft.Extensions.Time.Testing;

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
		}, _pool);
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

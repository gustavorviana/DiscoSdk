using DiscoSdk.Hosting;
using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Hosting.Gateway.Shards;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using Microsoft.Extensions.Time.Testing;

namespace DiscoSdk.Hosting.Tests.Gateway.Shards;

/// <summary>
/// AutoReconnect = false: the shard reports the fault via the listener (so the user-facing
/// GatewayDisconnected event can fire upstream) but does NOT auto-retry. A manual StartAsync
/// (the path the pool's reinit goes through, equivalent to IDiscordClient.ReconnectAsync at the
/// client level) is what brings it back.
/// </summary>
public class ShardAutoReconnectTests
{
	private readonly FakeGatewaySocket _socket = new();
	private readonly FakeTimeProvider _time = new();
	private readonly FakeShardPool _pool;
	private readonly Shard _shard;

	public ShardAutoReconnectTests()
	{
		_pool = new FakeShardPool(_socket, _time);
		_shard = new Shard(0, new DiscordClientConfig
		{
			Token = "test-token",
			Intents = DiscordIntent.Guilds,
			ReconnectDelay = TimeSpan.FromSeconds(5),
			AutoReconnect = false,
			HeartbeatJitter = 0.0,
			ReconnectBackoffJitter = 0.0,
		}, _pool);
	}

	[Fact]
	public async Task MissedHeartbeat_WithAutoReconnectFalse_StaysInConnectionLostAsync()
	{
		await _shard.StartAsync();
		await _socket.EnqueueInbound(TestFrames.Hello(heartbeatIntervalMs: 5000));
		await _socket.EnqueueInbound(TestFrames.Ready());
		await WaitFor(() => _shard.Status == ShardStatus.Ready);
		await WaitForOpcodeCount(OpCodes.Heartbeat, 1);

		// Tick past the heartbeat interval → fault published, listener notified, but no retry.
		_time.Advance(TimeSpan.FromSeconds(5));
		await WaitFor(() => _pool.ConnectionLostEvents.Count > 0);

		// Give the (would-be) reconnect delay plenty of virtual time — nothing should happen.
		_time.Advance(TimeSpan.FromSeconds(30));
		await Task.Delay(100);

		Assert.Equal(1, _socket.ConnectCount);
		Assert.Equal(ShardStatus.Disconnected, _shard.Status);
	}

	[Fact]
	public async Task ManualStartAsync_AfterAutoReconnectDisabledFault_ReconnectsAsync()
	{
		await _shard.StartAsync();
		await _socket.EnqueueInbound(TestFrames.Hello(heartbeatIntervalMs: 5000));
		await _socket.EnqueueInbound(TestFrames.Ready());
		await WaitFor(() => _shard.Status == ShardStatus.Ready);
		await WaitForOpcodeCount(OpCodes.Heartbeat, 1);

		_time.Advance(TimeSpan.FromSeconds(5));
		await WaitFor(() => _shard.Status == ShardStatus.Disconnected);

		// Pool-level reinit (what IDiscordClient.ReconnectAsync triggers via ClearShardsAsync +
		// InitShardsAsync) drives shards through StartAsync.
		await _shard.StartAsync();
		await WaitFor(() => _socket.ConnectCount >= 2);

		Assert.Equal(_pool.GatewayUri.ToUri(), _socket.ConnectedTo);
	}

	private async Task WaitForOpcodeCount(OpCodes op, int expected, int timeoutMs = 2000)
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
}

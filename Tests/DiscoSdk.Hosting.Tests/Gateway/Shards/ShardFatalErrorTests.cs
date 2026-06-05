using DiscoSdk.Hosting;
using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Hosting.Gateway.Shards;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using Microsoft.Extensions.Time.Testing;
using System.Net.WebSockets;

namespace DiscoSdk.Hosting.Tests.Gateway.Shards;

/// <summary>
/// Non-transport exceptions (JSON parse errors, internal bugs, etc.) cannot be recovered by the
/// shard's reconnect path. They must route through <see cref="IShardEventListener.OnFatalAsync"/>
/// and exit the run loop so the client can rethrow them on WaitShutdownAsync / WaitReadyAsync.
/// </summary>
public class ShardFatalErrorTests
{
	private readonly FakeGatewaySocket _socket = new();
	private readonly FakeTimeProvider _time = new();
	private readonly FakeShardPool _pool;
	private readonly Shard _shard;

	public ShardFatalErrorTests()
	{
		_pool = new FakeShardPool(_socket, _time);
		_shard = new Shard(0, new DiscordClientConfig
		{
			Token = "test-token",
			Intents = DiscordIntent.Guilds,
			ReconnectDelay = TimeSpan.FromSeconds(5),
			HeartbeatJitter = 0.0,
			ReconnectBackoffJitter = 0.0,
		}, _pool);
	}

	[Fact]
	public async Task NonTransportException_RoutesToOnFatalAsync_AndStopsLoopAsync()
	{
		await _shard.StartAsync();
		await _socket.EnqueueInbound(TestFrames.Hello());
		await _socket.EnqueueInbound(TestFrames.Ready());
		await WaitFor(() => _shard.Status == ShardStatus.Ready);

		// Inject a non-transport exception — the shard's catch must promote it to fatal.
		var bug = new InvalidOperationException("simulated dispatcher bug");
		_socket.InjectReadFault(bug);

		await WaitFor(() => _pool.FatalEvents.Count > 0);
		Assert.Same(bug, _pool.FatalEvents[0]);

		// Run loop exited — no reconnect attempt.
		_time.Advance(TimeSpan.FromMinutes(1));
		await Task.Delay(50);
		Assert.Equal(1, _socket.ConnectCount);
	}

	[Fact]
	public async Task FatalCloseCode_RoutesToOnFatalAsync_AndStopsLoopAsync()
	{
		await _shard.StartAsync();
		await _socket.EnqueueInbound(TestFrames.Hello());
		await _socket.EnqueueInbound(TestFrames.Ready());
		await WaitFor(() => _shard.Status == ShardStatus.Ready);

		// 4004 = authentication failed. The recovery path cannot fix this — must terminate.
		var fatal = new DiscordSocketException((WebSocketCloseStatus)4004, "Authentication failed.");
		_socket.InjectReadFault(fatal);

		await WaitFor(() => _pool.FatalEvents.Count > 0);
		Assert.Same(fatal, _pool.FatalEvents[0]);

		// Run loop exited — no reconnect attempt even with AutoReconnect on.
		_time.Advance(TimeSpan.FromMinutes(15));
		await Task.Delay(50);
		Assert.Equal(1, _socket.ConnectCount);
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

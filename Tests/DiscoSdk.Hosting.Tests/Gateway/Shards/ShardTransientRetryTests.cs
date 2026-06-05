using DiscoSdk.Hosting;
using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Hosting.Gateway.Shards;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using Microsoft.Extensions.Time.Testing;
using System.Net.WebSockets;

namespace DiscoSdk.Hosting.Tests.Gateway.Shards;

/// <summary>
/// A reconnect attempt that itself fails (server still unreachable) must not silently kill
/// <c>RunLoopAsync</c>. The catch wraps each attempt in its own retry loop and escalates the
/// backoff until either it succeeds, the cancellation token fires, or a fatal close code lands.
/// </summary>
public class ShardTransientRetryTests
{
	private readonly FakeGatewaySocket _socket = new();
	private readonly FakeTimeProvider _time = new();
	private readonly FakeShardPool _pool;
	private readonly Shard _shard;
	private readonly TimeSpan _baseDelay = TimeSpan.FromSeconds(5);

	public ShardTransientRetryTests()
	{
		_pool = new FakeShardPool(_socket, _time);
		_shard = new Shard(0, new DiscordClientConfig
		{
			Token = "test-token",
			Intents = DiscordIntent.Guilds,
			ReconnectDelay = _baseDelay,
			HeartbeatJitter = 0.0,
			ReconnectBackoffJitter = 0.0,
		}, _pool);
	}

	[Fact]
	public async Task TransientReconnectFailure_EmitsReconnectingEventPerAttemptAsync()
	{
		await _shard.StartAsync();
		await _socket.EnqueueInbound(TestFrames.Hello());
		await _socket.EnqueueInbound(TestFrames.Ready());
		await WaitFor(() => _shard.Status == ShardStatus.Ready);

		// Queue two failures so the retry loop runs at least three iterations: 5s, 10s, 20s.
		_socket.QueueConnectFault(new WebSocketException("down"));
		_socket.QueueConnectFault(new WebSocketException("still down"));
		_socket.InjectReadFault(new WebSocketException("primary dropped"));
		await WaitFor(() => _pool.ConnectionLostEvents.Count > 0);

		await AdvanceUntil(() => _socket.ConnectCount >= 4, TimeSpan.FromSeconds(2), timeoutMs: 5000);

		// One Reconnecting event fired per attempt, attempts numbered 1..N, delays climb exponentially.
		Assert.True(_pool.ReconnectingEvents.Count >= 3);
		Assert.Equal(1, _pool.ReconnectingEvents[0].Attempt);
		Assert.Equal(2, _pool.ReconnectingEvents[1].Attempt);
		Assert.Equal(3, _pool.ReconnectingEvents[2].Attempt);
		Assert.Equal(TimeSpan.FromSeconds(5), _pool.ReconnectingEvents[0].Delay);
		Assert.Equal(TimeSpan.FromSeconds(10), _pool.ReconnectingEvents[1].Delay);
		Assert.Equal(TimeSpan.FromSeconds(20), _pool.ReconnectingEvents[2].Delay);
	}

	[Fact]
	public async Task TransientReconnectFailure_KeepsRetryingUntilConnectAsync()
	{
		await _shard.StartAsync();
		await _socket.EnqueueInbound(TestFrames.Hello());
		await _socket.EnqueueInbound(TestFrames.Ready());
		await WaitFor(() => _shard.Status == ShardStatus.Ready);

		// Queue two transient failures — the first two ConnectAsync attempts on the way back up
		// will throw. The third must succeed; the shard cannot give up silently in between.
		_socket.QueueConnectFault(new WebSocketException("server unreachable"));
		_socket.QueueConnectFault(new WebSocketException("server still unreachable"));

		// Drop the live connection. ConnectCount == 1 right now.
		_socket.InjectReadFault(new WebSocketException("primary connection dropped"));
		await WaitFor(() => _pool.ConnectionLostEvents.Count > 0);

		// First attempt fires after baseline backoff (5s). Fails with the first queued fault.
		await AdvanceUntil(() => _socket.ConnectCount >= 2, TimeSpan.FromSeconds(1), timeoutMs: 5000);

		// Second attempt fires after backoff doubles to 10s. Fails with the second queued fault.
		await AdvanceUntil(() => _socket.ConnectCount >= 3, TimeSpan.FromSeconds(2), timeoutMs: 5000);

		// Third attempt at backoff = 20s. Queue is drained, succeeds.
		await AdvanceUntil(() => _socket.ConnectCount >= 4, TimeSpan.FromSeconds(2), timeoutMs: 5000);

		Assert.Equal(_pool.GatewayUri.ToUri(), _socket.ConnectedTo);
		// Fatal channel must NOT have fired — transient failures are recoverable.
		Assert.Empty(_pool.FatalEvents);
	}

	[Fact]
	public async Task FatalCloseCodeDuringRetry_RoutesToOnFatalAsync_AndStopsAsync()
	{
		await _shard.StartAsync();
		await _socket.EnqueueInbound(TestFrames.Hello());
		await _socket.EnqueueInbound(TestFrames.Ready());
		await WaitFor(() => _shard.Status == ShardStatus.Ready);

		// Drop the live connection (recoverable), then have the very next ConnectAsync answer with
		// a fatal close code (4014 — disallowed intents). The retry loop must short-circuit.
		_socket.QueueConnectFault(new DiscordSocketException((WebSocketCloseStatus)4014, "Disallowed intents"));
		_socket.InjectReadFault(new WebSocketException("primary connection dropped"));

		await WaitFor(() => _pool.ConnectionLostEvents.Count > 0);
		await AdvanceUntil(() => _pool.FatalEvents.Count > 0, TimeSpan.FromSeconds(1), timeoutMs: 10_000);

		Assert.IsType<DiscordSocketException>(_pool.FatalEvents[0]);
		Assert.Equal(4014, ((DiscordSocketException)_pool.FatalEvents[0]).CloseCode);

		// No further connect attempts.
		var snapshot = _socket.ConnectCount;
		_time.Advance(TimeSpan.FromMinutes(10));
		await Task.Delay(50);
		Assert.Equal(snapshot, _socket.ConnectCount);
	}

	private async Task AdvanceUntil(Func<bool> condition, TimeSpan step, int timeoutMs)
	{
		var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
		while (DateTime.UtcNow < deadline)
		{
			if (condition()) return;
			_time.Advance(step);
			await Task.Delay(20);
		}
		throw new TimeoutException("Condition not met within timeout.");
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

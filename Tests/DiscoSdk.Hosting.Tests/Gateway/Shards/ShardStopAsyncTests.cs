using DiscoSdk.Hosting;
using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Hosting.Gateway.Shards;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using Microsoft.Extensions.Time.Testing;
using System.Net.WebSockets;

namespace DiscoSdk.Hosting.Tests.Gateway.Shards;

/// <summary>
/// Per-shard CTS contract: StopAsync must actually terminate the run loop even when
/// AutoReconnect is on, otherwise a "stopped" shard keeps spinning the retry loop forever and
/// leaks a background task plus its sockets.
/// </summary>
public class ShardStopAsyncTests
{
	private readonly FakeGatewaySocket _socket = new();
	private readonly FakeTimeProvider _time = new();
	private readonly FakeShardPool _pool;
	private readonly Shard _shard;

	public ShardStopAsyncTests()
	{
		_pool = new FakeShardPool(_socket, _time);
		_shard = new Shard(0, new DiscordClientConfig
		{
			Token = "test-token",
			Intents = DiscordIntent.Guilds,
			ReconnectDelay = TimeSpan.FromSeconds(5),
			AutoReconnect = true,
			HeartbeatJitter = 0.0,
			ReconnectBackoffJitter = 0.0,
		}, _pool);
	}

	[Fact]
	public async Task StopAsync_WithAutoReconnectOn_ExitsRunLoopAndDoesNotRetryAsync()
	{
		await _shard.StartAsync();
		await _socket.EnqueueInbound(TestFrames.Hello());
		await _socket.EnqueueInbound(TestFrames.Ready());
		await WaitForAsync(() => _shard.Status == ShardStatus.Ready);

		var connectsBefore = _socket.ConnectCount;

		await _shard.StopAsync();

		// Generous virtual time to expose any zombie retry: 30 minutes well past the 900s backoff cap.
		_time.Advance(TimeSpan.FromMinutes(30));
		await Task.Delay(100);

		Assert.Equal(connectsBefore, _socket.ConnectCount);
		Assert.Equal(ShardStatus.Disconnected, _shard.Status);
	}

	[Fact]
	public async Task StopAsync_DuringRetryBackoff_ExitsImmediatelyAsync()
	{
		await _shard.StartAsync();
		await _socket.EnqueueInbound(TestFrames.Hello());
		await _socket.EnqueueInbound(TestFrames.Ready());
		await WaitForAsync(() => _shard.Status == ShardStatus.Ready);

		// Drop the connection so the shard enters Reconnecting.
		_socket.InjectReadFault(new WebSocketException("drop"));
		await WaitForAsync(() => _shard.Status == ShardStatus.Reconnecting);

		var connectsBefore = _socket.ConnectCount;

		// StopAsync must cancel the backoff sleep immediately.
		await _shard.StopAsync();

		_time.Advance(TimeSpan.FromMinutes(30));
		await Task.Delay(100);

		Assert.Equal(connectsBefore, _socket.ConnectCount);
		Assert.Equal(ShardStatus.Disconnected, _shard.Status);
	}

	private static async Task WaitForAsync(Func<bool> condition, int timeoutMs = 2000)
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

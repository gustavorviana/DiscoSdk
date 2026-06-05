using DiscoSdk.Hosting;
using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Hosting.Gateway.Shards;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using Microsoft.Extensions.Time.Testing;

namespace DiscoSdk.Hosting.Tests.Gateway.Shards;

/// <summary>
/// Regression guard for the <see cref="Shard.StartAsync"/> early-return check. The previous
/// expression (<c>is not PendingHello and ConnectionLost</c>) parsed as <c>(is not PendingHello) AND
/// (is ConnectionLost)</c> — which let a re-entrant <c>StartAsync</c> spawn a second connection on
/// a Ready shard, and which silently refused a manual restart on a ConnectionLost shard. The fix
/// is <c>is not (PendingHello or ConnectionLost)</c>.
/// </summary>
public class ShardStartGuardTests
{
	private readonly FakeGatewaySocket _socket = new();
	private readonly FakeTimeProvider _time = new();
	private readonly FakeShardPool _pool;
	private readonly Shard _shard;

	public ShardStartGuardTests()
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
	public async Task StartAsync_WhenAlreadyReady_DoesNotReconnectAsync()
	{
		await _shard.StartAsync();
		await _socket.EnqueueInbound(TestFrames.Hello());
		await _socket.EnqueueInbound(TestFrames.Ready());
		await WaitFor(() => _shard.Status == ShardStatus.Ready);

		var connectsBefore = _socket.ConnectCount;
		await _shard.StartAsync();

		Assert.Equal(connectsBefore, _socket.ConnectCount);
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

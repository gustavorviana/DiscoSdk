using DiscoSdk.Hosting;
using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Hosting.Gateway.Shards;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using Microsoft.Extensions.Time.Testing;
using System.Net.WebSockets;

namespace DiscoSdk.Hosting.Tests.Gateway.Shards;

/// <summary>
/// The IdentifyGate permit is acquired in SetupIdentifyAsync (status flips to Identifying) and
/// released by SetReady on READY/RESUMED. If a transport drop arrives between those points the
/// permit must still be released, otherwise a multi-shard bot deadlocks: shard N grabs a permit,
/// drops mid-identify, gate slot leaks, and shards N+1..M wait forever.
/// </summary>
public class ShardIdentifyGateTests
{
	private readonly FakeGatewaySocket _socket = new();
	private readonly FakeTimeProvider _time = new();
	private readonly FakeShardPool _pool;
	private readonly Shard _shard;

	public ShardIdentifyGateTests()
	{
		_pool = new FakeShardPool(_socket, _time);
		_shard = new Shard(0, new DiscordClientConfig
		{
			Token = "test-token",
			Intents = DiscordIntent.Guilds,
			ReconnectDelay = TimeSpan.FromSeconds(5),
			AutoReconnect = false, // keep shard stationary in Disconnected after drop for assertion
			HeartbeatJitter = 0.0,
			ReconnectBackoffJitter = 0.0,
		}, _pool);
	}

	[Fact]
	public async Task DisconnectDuringIdentifying_ReleasesGatePermitAsync()
	{
		await _shard.StartAsync();
		await _socket.EnqueueInbound(TestFrames.Hello());

		// Shard sends IDENTIFY → flips to Identifying. Wait for the status transition rather than
		// the frame; the SetupIdentifyAsync await on Gate.WaitAsync is what acquires the permit.
		await WaitForAsync(() => _shard.Status == ShardStatus.Identifying);
		Assert.Equal(1, _pool.Gate.PendingReleaseCount);

		// Drop the connection BEFORE the READY frame arrives. SignalConnectionLost must release
		// the permit even though SetReady never ran.
		_socket.InjectReadFault(new WebSocketException("drop mid-identify"));
		await WaitForAsync(() => _shard.Status == ShardStatus.Disconnected);

		Assert.Equal(0, _pool.Gate.PendingReleaseCount);
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

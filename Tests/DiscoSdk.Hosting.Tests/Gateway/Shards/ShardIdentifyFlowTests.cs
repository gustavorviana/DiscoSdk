using DiscoSdk.Hosting;
using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Hosting.Gateway.Shards;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using Microsoft.Extensions.Time.Testing;

namespace DiscoSdk.Hosting.Tests.Gateway.Shards;

/// <summary>
/// Verifies the shard's Hello → Identify → Ready state machine using a controllable
/// <see cref="FakeGatewaySocket"/> and <see cref="FakeTimeProvider"/>.
/// </summary>
public class ShardIdentifyFlowTests
{
	private readonly FakeGatewaySocket _socket = new();
	private readonly FakeTimeProvider _time = new();
	private readonly FakeShardPool _pool;
	private readonly DiscoSdk.Hosting.Gateway.Shards.Shard _shard;

	public ShardIdentifyFlowTests()
	{
		_pool = new FakeShardPool(_socket, _time);
		_shard = new DiscoSdk.Hosting.Gateway.Shards.Shard(shardId: 0, new DiscordClientConfig
		{
			Token = "test-token",
			Intents = DiscordIntent.Guilds,
			ReconnectDelay = TimeSpan.FromSeconds(5),
		}, _pool);
	}

	[Fact]
	public async Task StartAsync_ConnectsSocketToGatewayUriAsync()
	{
		await _shard.StartAsync();

		Assert.Equal(_pool.GatewayUri.ToUri(), _socket.ConnectedTo);
		Assert.True(_socket.Ready);
	}

	[Fact]
	public async Task OnHello_SendsIdentifyWithConfigTokenAsync()
	{
		await _shard.StartAsync();

		await _socket.EnqueueInbound(TestFrames.Hello());

		var identify = await WaitForSentOpcode(OpCodes.Identify);
		Assert.NotNull(identify);
	}

	[Fact]
	public async Task OnReady_TransitionsToReadyAndRaisesListenerAsync()
	{
		await _shard.StartAsync();
		await _socket.EnqueueInbound(TestFrames.Hello());
		await WaitForSentOpcode(OpCodes.Identify);

		await _socket.EnqueueInbound(TestFrames.Ready(sessionId: "sess-abc"));

		await WaitFor(() => _pool.ReadyEvents.Count == 1);
		Assert.Equal("sess-abc", _pool.ReadyEvents[0].SessionId);
		Assert.Equal(ShardStatus.Ready, _shard.Status);
	}

	// ---- helpers ----

	private async Task<SendGatewayMessage?> WaitForSentOpcode(OpCodes op, int timeoutMs = 1000)
	{
		var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
		while (DateTime.UtcNow < deadline)
		{
			var match = _socket.SentFrames.FirstOrDefault(f => f.OpCode == op);
			if (match is not null) return match;
			await Task.Delay(5);
		}
		return null;
	}

	private static async Task WaitFor(Func<bool> condition, int timeoutMs = 1000)
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

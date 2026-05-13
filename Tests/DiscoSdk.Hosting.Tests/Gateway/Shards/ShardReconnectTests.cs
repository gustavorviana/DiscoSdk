using DiscoSdk.Hosting;
using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using Microsoft.Extensions.Time.Testing;

namespace DiscoSdk.Hosting.Tests.Gateway.Shards;

/// <summary>
/// Verifies the shard's response to OpCode 7 (Reconnect) and OpCode 9 (InvalidSession). The
/// reconnect delay is driven by <see cref="FakeTimeProvider"/>, so the test never sleeps.
/// </summary>
public class ShardReconnectTests
{
	private readonly FakeGatewaySocket _socket = new();
	private readonly FakeTimeProvider _time = new();
	private readonly FakeShardPool _pool;
	private readonly DiscoSdk.Hosting.Gateway.Shards.Shard _shard;
	private readonly TimeSpan _reconnectDelay = TimeSpan.FromSeconds(2);

	public ShardReconnectTests()
	{
		_pool = new FakeShardPool(_socket, _time);
		_shard = new DiscoSdk.Hosting.Gateway.Shards.Shard(0, new DiscordClientConfig
		{
			Token = "test-token",
			Intents = DiscordIntent.Guilds,
			ReconnectDelay = _reconnectDelay,
		}, _pool);
	}

	[Fact]
	public async Task OnReconnectOpcode_ClosesSocketAndWaitsReconnectDelayAsync()
	{
		await _shard.StartAsync();
		await _socket.EnqueueInbound(TestFrames.Hello());
		await _socket.EnqueueInbound(TestFrames.Ready(sessionId: "sess-1", resumeGatewayUrl: "wss://resume.test/"));
		await WaitFor(() => _shard.Status == DiscoSdk.Hosting.Gateway.Shards.ShardStatus.Ready);

		await _socket.EnqueueInbound(TestFrames.Reconnect());

		// The shard closes the socket as the first step of the reconnect flow.
		await WaitFor(() => _socket.Closed);

		// Advance past the reconnect delay so the shard proceeds to ConnectAsync on the resume URL.
		await AdvanceUntil(() => _socket.ConnectCount >= 2, _reconnectDelay);

		// Second connect is to the resume URL provided in the READY payload.
		Assert.Equal(new Uri("wss://resume.test/"), _socket.ConnectedTo);
	}

	[Fact]
	public async Task OnInvalidSessionNotResumable_ReconnectsToOriginalGatewayUriAsync()
	{
		await _shard.StartAsync();
		await _socket.EnqueueInbound(TestFrames.Hello());
		await _socket.EnqueueInbound(TestFrames.Ready());
		await WaitFor(() => _shard.Status == DiscoSdk.Hosting.Gateway.Shards.ShardStatus.Ready);

		// Non-resumable invalid session → shard wipes session id and reconnects to the pool's
		// gateway URI (not the resume URL).
		await _socket.EnqueueInbound(TestFrames.InvalidSession(resumable: false));
		await WaitFor(() => _socket.Closed);

		await AdvanceUntil(() => _socket.ConnectCount >= 2, _reconnectDelay);

		Assert.Equal(_pool.GatewayUri.ToUri(), _socket.ConnectedTo);
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

	/// <summary>
	/// Polls <paramref name="condition"/> and repeatedly advances <see cref="_time"/> by
	/// <paramref name="step"/> until it's true. Used when the shard's reconnect flow registers a
	/// <c>Task.Delay(reconnectDelay, timeProvider, ...)</c> at an unknown moment after the test's
	/// previous await — a single advance would no-op if the timer isn't registered yet.
	/// </summary>
	private async Task AdvanceUntil(Func<bool> condition, TimeSpan step, int timeoutMs = 2000)
	{
		var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
		while (DateTime.UtcNow < deadline)
		{
			if (condition()) return;
			_time.Advance(step);
			await Task.Delay(5);
		}
		throw new TimeoutException("Condition not met within timeout.");
	}
}

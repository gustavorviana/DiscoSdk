using DiscoSdk;
using DiscoSdk.Events;
using DiscoSdk.Hosting.Builders;
using DiscoSdk.Hosting.Gateway.Shards;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using DiscoSdk.Rest;
using NSubstitute;
using System.Net.WebSockets;

namespace DiscoSdk.Hosting.Tests;

/// <summary>
/// Verifies the public <see cref="IDiscordClient.GatewayDisconnected"/> event surface: a handler
/// subscribed at the client level sees the shard, the underlying exception, and what the SDK plans
/// to do (<see cref="GatewayDisconnectedEventArgs.WillReconnect"/>).
/// </summary>
public class GatewayDisconnectedEventTests
{
	private static DiscordClient BuildClient(bool autoReconnect)
	{
		var http = Substitute.For<IDiscordRestClient>();
		return DiscordClientBuilder.Create("test-token")
			.WithIntents(DiscordIntent.Guilds)
			.WithRestClient(http)
			.WithAutoReconnect(autoReconnect)
			.Build();
	}

	[Fact]
	public async Task GatewayDisconnected_FiresWithShardAndException_WhenAutoReconnectOnAsync()
	{
		var client = BuildClient(autoReconnect: true);
		client.SeedShardsForTests(totalShards: 1);
		var shard = client.GetShard(0);

		GatewayDisconnectedEventArgs? observed = null;
		client.GatewayDisconnected += args =>
		{
			observed = args;
			return Task.CompletedTask;
		};

		var ex = new WebSocketException("simulated drop");
		await ((IShardEventListener)client).OnConnectionLostAsync(shard, ex);

		Assert.NotNull(observed);
		Assert.Same(shard, observed!.Shard);
		Assert.Equal(0, observed.Shard.Id);
		Assert.Same(ex, observed.Exception);
		Assert.True(observed.WillReconnect);
	}

	[Fact]
	public async Task GatewayDisconnected_WillReconnectIsFalse_WhenAutoReconnectDisabledAsync()
	{
		var client = BuildClient(autoReconnect: false);
		client.SeedShardsForTests(totalShards: 1);
		var shard = client.GetShard(0);

		GatewayDisconnectedEventArgs? observed = null;
		client.GatewayDisconnected += args =>
		{
			observed = args;
			return Task.CompletedTask;
		};

		await ((IShardEventListener)client).OnConnectionLostAsync(shard, new WebSocketException("drop"));

		Assert.NotNull(observed);
		Assert.False(observed!.WillReconnect);
	}

	[Fact]
	public async Task GatewayDisconnected_WillReconnectIsFalse_WhenExceptionIsFatalAsync()
	{
		// Non-transport exception → shard's catch routes to OnFatalAsync (no reconnect). The args
		// must reflect that even when AutoReconnect = true, otherwise the bot author thinks a retry
		// is coming and silently doesn't get one.
		var client = BuildClient(autoReconnect: true);
		client.SeedShardsForTests(totalShards: 1);
		var shard = client.GetShard(0);

		GatewayDisconnectedEventArgs? observed = null;
		client.GatewayDisconnected += args =>
		{
			observed = args;
			return Task.CompletedTask;
		};

		await ((IShardEventListener)client).OnConnectionLostAsync(shard, new InvalidOperationException("internal bug"));

		Assert.NotNull(observed);
		Assert.False(observed!.WillReconnect);
	}

	[Fact]
	public async Task GatewayDisconnected_ShardOnArgs_IsSameInstanceAsClientShardsMemberAsync()
	{
		// Identity contract: the IShard handed to a handler is the same instance the bot finds
		// via client.Shards, so observability (Id / IsReady) tracks one object across both paths.
		var client = BuildClient(autoReconnect: false);
		client.SeedShardsForTests(totalShards: 1);
		var shard = client.GetShard(0);

		IShard? observed = null;
		client.GatewayDisconnected += args =>
		{
			observed = args.Shard;
			return Task.CompletedTask;
		};

		await ((IShardEventListener)client).OnConnectionLostAsync(shard, new WebSocketException("drop"));

		Assert.NotNull(observed);
		Assert.Same(client.Shards[0], observed);
		Assert.Equal(0, observed!.Id);
	}

	[Fact]
	public async Task GatewayDisconnected_AwaitsAsyncHandlersBeforeReturningAsync()
	{
		var client = BuildClient(autoReconnect: true);
		client.SeedShardsForTests(totalShards: 1);
		var shard = client.GetShard(0);

		var gate = new TaskCompletionSource();
		var handlerStarted = false;
		var handlerFinished = false;
		client.GatewayDisconnected += async _ =>
		{
			handlerStarted = true;
			await gate.Task;
			handlerFinished = true;
		};

		var dispatch = Task.Run(() => ((IShardEventListener)client).OnConnectionLostAsync(shard, new WebSocketException("drop")));

		await WaitForAsync(() => handlerStarted);
		Assert.False(handlerFinished);
		Assert.False(dispatch.IsCompleted);

		gate.SetResult();
		await dispatch;

		Assert.True(handlerFinished);
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

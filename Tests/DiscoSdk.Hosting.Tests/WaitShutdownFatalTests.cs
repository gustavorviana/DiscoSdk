using DiscoSdk;
using DiscoSdk.Exceptions;
using DiscoSdk.Hosting.Builders;
using DiscoSdk.Hosting.Gateway.Shards;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests;

/// <summary>
/// Fatal errors caught by a shard propagate up to the WaitShutdownAsync / WaitReadyAsync surface
/// as <see cref="DiscordFatalException"/>. Without this, a bot's entry point that does
/// <c>await client.WaitShutdownAsync()</c> would return as if shutdown was graceful even when a
/// shard died from an unrecoverable error.
/// </summary>
public class WaitShutdownFatalTests
{
	private static DiscordClient BuildClient()
	{
		var http = Substitute.For<IDiscordRestClient>();
		var client = DiscordClientBuilder.Create("test-token")
			.WithIntents(DiscordIntent.Guilds)
			.WithRestClient(http)
			.Build();
		client.SeedShardsForTests(totalShards: 1);
		return client;
	}

	private static async Task SignalFatalAsync(DiscordClient client, Exception cause)
		=> await ((IShardEventListener)client).OnFatalAsync(client.GetShard(0), cause);

	[Fact]
	public async Task WaitShutdownAsync_AfterFatal_ThrowsDiscordFatalExceptionAsync()
	{
		var client = BuildClient();
		var cause = new InvalidOperationException("internal bug");

		await SignalFatalAsync(client, cause);

		var ex = await Assert.ThrowsAsync<DiscordFatalException>(() => client.WaitShutdownAsync());
		Assert.Same(cause, ex.InnerException);
	}

	[Fact]
	public async Task WaitShutdownAsync_WithTimeout_AfterFatal_ThrowsDiscordFatalExceptionAsync()
	{
		var client = BuildClient();
		var cause = new InvalidOperationException("internal bug");

		await SignalFatalAsync(client, cause);

		var ex = await Assert.ThrowsAsync<DiscordFatalException>(() => client.WaitShutdownAsync(TimeSpan.FromSeconds(5)));
		Assert.Same(cause, ex.InnerException);
	}

	[Fact]
	public async Task WaitReadyAsync_AfterFatal_ThrowsDiscordFatalExceptionAsync()
	{
		var client = BuildClient();
		var cause = new InvalidOperationException("internal bug");

		await SignalFatalAsync(client, cause);

		var ex = await Assert.ThrowsAsync<DiscordFatalException>(() => client.WaitReadyAsync());
		Assert.Same(cause, ex.InnerException);
	}

	[Fact]
	public async Task GracefulShutdown_DoesNotThrowAsync()
	{
		var client = BuildClient();

		await client.StopAsync();
		await client.WaitShutdownAsync();
		// No throw — clean exit.
	}

	[Fact]
	public async Task FirstFatalCauseIsPreserved_WhenMultipleShardsFailAsync()
	{
		var client = BuildClient();
		var first = new InvalidOperationException("first");
		var second = new ApplicationException("second");

		await SignalFatalAsync(client, first);
		await SignalFatalAsync(client, second);

		var ex = await Assert.ThrowsAsync<DiscordFatalException>(() => client.WaitShutdownAsync());
		Assert.Same(first, ex.InnerException);
	}
}

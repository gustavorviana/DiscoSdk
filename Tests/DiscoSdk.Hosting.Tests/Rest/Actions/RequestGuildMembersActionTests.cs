using DiscoSdk.Events;
using DiscoSdk.Hosting.Builders;
using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System.Text.Json;

namespace DiscoSdk.Hosting.Tests.Rest.Actions;

/// <summary>
/// End-to-end tests for the <see cref="RequestGuildMembersAction"/> — uses a fake gateway socket
/// to capture the outbound op-8 and a real <c>MemberChunkCoordinator</c> + sinks to drive the
/// terminal operations.
/// </summary>
public class RequestGuildMembersActionTests
{
	private readonly FakeGatewaySocket _socket;
	private readonly DiscordClient _client;
	private readonly Snowflake _guildId = new(100);

	public RequestGuildMembersActionTests()
	{
		_socket = new FakeGatewaySocket();
		var http = Substitute.For<IDiscordRestClient>();
		http.JsonOptions.Returns(new JsonSerializerOptions());

		_client = DiscordClientBuilder.Create("test-token")
			.WithIntents(DiscordIntent.Guilds | DiscordIntent.GuildMembers | DiscordIntent.GuildPresences)
			.WithLogger(NullLogger.Instance)
			.WithGatewaySocketFactory(new FakeGatewaySocketFactory(_socket))
			.WithRestClient(http)
			.Build();

		// Seed a single shard so GetShardForGuild has somewhere to dispatch.
		_client.SeedShardsForTests(totalShards: 1);
	}

	private RequestGuildMembersAction NewAction() => new(_client, _guildId);

	private string? LastSentNonce()
	{
		var frame = _socket.SentFrames.Single(f => f.OpCode == OpCodes.RequestGuildMembers);
		var data = frame.Data!;
		var nonceProp = data.GetType().GetProperty("Nonce");
		return (string?)nonceProp!.GetValue(data);
	}

	[Fact]
	public async Task GetAsync_BuffersAllChunksAndReturnsListAsync()
	{
		var task = NewAction().SetQuery("alice").GetAsync();

		await WaitForAsync(() => _socket.SentFrames.Any(f => f.OpCode == OpCodes.RequestGuildMembers));
		var nonce = LastSentNonce()!;

		// Two-chunk session.
		var first = new IMember[] { Substitute.For<IMember>(), Substitute.For<IMember>() };
		var second = new IMember[] { Substitute.For<IMember>() };
		_client.MemberChunkCoordinator.TryDeliver(nonce, first, chunkIndex: 0, chunkCount: 2);
		_client.MemberChunkCoordinator.TryDeliver(nonce, second, chunkIndex: 1, chunkCount: 2);

		var members = await task.WaitAsync(TimeSpan.FromSeconds(2));
		Assert.Equal(3, members.Count);
	}

	[Fact]
	public async Task StreamAsync_YieldsMembersIndividuallyAsync()
	{
		var action = NewAction().SetUserIds(new Snowflake(42), new Snowflake(43));

		var enumerator = action.StreamAsync().GetAsyncEnumerator();

		try
		{
			// Async iterators only start running on the first MoveNextAsync. Kick it off in
			// the background so the op-8 send actually fires.
			var firstMove = enumerator.MoveNextAsync().AsTask();

			await WaitForAsync(() => _socket.SentFrames.Any(f => f.OpCode == OpCodes.RequestGuildMembers));
			var nonce = LastSentNonce()!;

			var members = new IMember[]
			{
				Substitute.For<IMember>(),
				Substitute.For<IMember>(),
				Substitute.For<IMember>(),
			};
			_client.MemberChunkCoordinator.TryDeliver(nonce, members, chunkIndex: 0, chunkCount: 1);

			var received = new List<IMember>();
			if (await firstMove.WaitAsync(TimeSpan.FromSeconds(2)))
			{
				received.Add(enumerator.Current);
				while (await enumerator.MoveNextAsync())
					received.Add(enumerator.Current);
			}

			Assert.Equal(3, received.Count);
		}
		finally
		{
			await enumerator.DisposeAsync();
		}
	}

	[Fact]
	public async Task StreamAsync_EarlyBreak_CancelsPendingRequestAsync()
	{
		var action = NewAction();
		var enumerator = action.StreamAsync().GetAsyncEnumerator();

		// Kick the iterator so the op-8 send runs.
		var firstMove = enumerator.MoveNextAsync().AsTask();

		await WaitForAsync(() => _socket.SentFrames.Any(f => f.OpCode == OpCodes.RequestGuildMembers));
		var nonce = LastSentNonce()!;

		// Deliver one member but mark this chunk as non-final so the channel stays open.
		_client.MemberChunkCoordinator.TryDeliver(nonce, [Substitute.For<IMember>()], chunkIndex: 0, chunkCount: 5);

		Assert.True(await firstMove.WaitAsync(TimeSpan.FromSeconds(2)));

		// Consumer "breaks" by disposing — should cancel the in-flight request.
		await enumerator.DisposeAsync();

		// After cancel, the coordinator has dropped the nonce — re-registering must succeed.
		_client.MemberChunkCoordinator.Register(nonce, new BufferingMemberChunkSink());
	}

	[Fact]
	public async Task GetAsync_CancellationToken_FaultsTaskAsync()
	{
		using var cts = new CancellationTokenSource();
		var task = NewAction().SetQuery("xyz").GetAsync(cts.Token);

		await WaitForAsync(() => _socket.SentFrames.Any(f => f.OpCode == OpCodes.RequestGuildMembers));
		cts.Cancel();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task.WaitAsync(TimeSpan.FromSeconds(2)));
	}

	[Fact]
	public async Task SendsOpEightWithCorrectPayloadAsync()
	{
		var task = NewAction()
			.SetQuery("alice")
			.SetLimit(50)
			.SetPresences(true)
			.GetAsync();

		await WaitForAsync(() => _socket.SentFrames.Any(f => f.OpCode == OpCodes.RequestGuildMembers));

		var frame = _socket.SentFrames.Single(f => f.OpCode == OpCodes.RequestGuildMembers);
		var data = frame.Data!;
		var t = data.GetType();

		Assert.Equal(_guildId.ToString(), t.GetProperty("GuildId")!.GetValue(data));
		Assert.Equal("alice", t.GetProperty("Query")!.GetValue(data));
		Assert.Equal(50, t.GetProperty("Limit")!.GetValue(data));
		Assert.Equal(true, t.GetProperty("Presences")!.GetValue(data));

		// Cleanup — complete the request so the test base disposal doesn't hang.
		var nonce = LastSentNonce()!;
		_client.MemberChunkCoordinator.TryDeliver(nonce, [], chunkIndex: 0, chunkCount: 1);
		await task.WaitAsync(TimeSpan.FromSeconds(2));
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

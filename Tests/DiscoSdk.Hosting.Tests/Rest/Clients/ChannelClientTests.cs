using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Rest.Clients;

public class ChannelClientTests
{
	private readonly IDiscordRestClient _http = Substitute.For<IDiscordRestClient>();
	private readonly ChannelClient _client;
	private readonly Snowflake _channelId = new(111);
	private readonly Snowflake _userId = new(222);

	public ChannelClientTests()
	{
		_http.JsonOptions.Returns(new System.Text.Json.JsonSerializerOptions());
		// MessageClient is required for delegated message ops — give it a real instance backed by the mock.
		var messageClient = new MessageClient(_http);
		_client = new ChannelClient(_http, messageClient);
	}

	[Fact]
	public async Task GetAsync_GetsChannelByIdRouteAsync()
	{
		_http.SendAsync<Channel>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Channel());

		await _client.GetAsync(_channelId);

		await _http.Received(1).SendAsync<Channel>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task EditAsync_PatchesChannelByIdRouteAsync()
	{
		_http.SendAsync<Channel>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Channel());

		var req = new { name = "x" };
		await _client.EditAsync(_channelId, req);

		await _http.Received(1).SendAsync<Channel>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}"),
			HttpMethod.Patch,
			req,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task DeleteAsync_DeletesChannelByIdRouteAsync()
	{
		await _client.DeleteAsync(_channelId);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task FollowAsync_PostsFollowersRouteWithTargetChannelIdAsync()
	{
		var targetId = new Snowflake(999);
		_http.SendAsync<FollowedChannel>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new FollowedChannel());

		await _client.FollowAsync(_channelId, targetId);

		await _http.Received(1).SendAsync<FollowedChannel>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}/followers"),
			HttpMethod.Post,
			Arg.Is<object?>(b =>
				b!.GetType().GetProperty("webhook_channel_id")!.GetValue(b)!.Equals(targetId.ToString())),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task JoinThreadAsync_PutsMeThreadMembersRouteAsync()
	{
		await _client.JoinThreadAsync(_channelId);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}/thread-members/@me"),
			HttpMethod.Put,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task LeaveThreadAsync_DeletesMeThreadMembersRouteAsync()
	{
		await _client.LeaveThreadAsync(_channelId);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}/thread-members/@me"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task AddThreadMemberAsync_PutsThreadMemberByUserIdAsync()
	{
		await _client.AddThreadMemberAsync(_channelId, _userId);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}/thread-members/{_userId}"),
			HttpMethod.Put,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task RemoveThreadMemberAsync_DeletesThreadMemberByUserIdAsync()
	{
		await _client.RemoveThreadMemberAsync(_channelId, _userId);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}/thread-members/{_userId}"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetThreadMembersAsync_AppendsLimitAndAfterAndWithMemberQueryAsync()
	{
		_http.SendAsync<ThreadMember[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _client.GetThreadMembersAsync(_channelId, limit: 50, after: new Snowflake(7), withMember: true);

		await _http.Received(1).SendAsync<ThreadMember[]>(
			Arg.Is<DiscordRoute>(r =>
				r.ToString().Contains("limit=50") &&
				r.ToString().Contains("after=7") &&
				r.ToString().Contains("with_member=true")),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ArchiveThreadAsync_PatchesArchivedTrueAsync()
	{
		await _client.ArchiveThreadAsync(_channelId);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}"),
			HttpMethod.Patch,
			Arg.Is<object?>(b => b!.GetType().GetProperty("archived")!.GetValue(b)!.Equals(true)),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task LockThreadAsync_PatchesLockedTrueAsync()
	{
		await _client.LockThreadAsync(_channelId);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}"),
			HttpMethod.Patch,
			Arg.Is<object?>(b => b!.GetType().GetProperty("locked")!.GetValue(b)!.Equals(true)),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateForumPostAsync_PostsToThreadsRouteAsync()
	{
		_http.SendAsync<ForumPost>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new ForumPost());

		var req = new { name = "post" };
		await _client.CreateForumPostAsync(_channelId, req);

		await _http.Received(1).SendAsync<ForumPost>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}/threads"),
			HttpMethod.Post,
			req,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateThreadFromMessageAsync_PostsToMessageThreadsRouteAsync()
	{
		var messageId = new Snowflake(444);
		_http.SendAsync<Channel>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Channel());

		var req = new { name = "t" };
		await _client.CreateThreadFromMessageAsync(_channelId, messageId, req);

		await _http.Received(1).SendAsync<Channel>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}/messages/{messageId}/threads"),
			HttpMethod.Post,
			req,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateDMAsync_PostsToMeChannelsRouteAsync()
	{
		_http.SendAsync<Channel>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Channel());

		await _client.CreateDMAsync(_userId);

		await _http.Received(1).SendAsync<Channel>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "users/@me/channels"),
			HttpMethod.Post,
			Arg.Is<object?>(b => b!.GetType().GetProperty("recipient_id")!.GetValue(b)!.Equals(_userId.ToString())),
			Arg.Any<CancellationToken>());
	}
}

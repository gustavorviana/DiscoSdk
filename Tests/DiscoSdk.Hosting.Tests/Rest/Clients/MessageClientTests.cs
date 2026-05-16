using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Models.Messages;
using DiscoSdk.Hosting.Models.Requests.Messages;
using DiscoSdk.Models.Users;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Rest.Clients;

public class MessageClientTests
{
	private readonly IDiscordRestClient _http = Substitute.For<IDiscordRestClient>();
	private readonly MessageClient _client;
	private readonly Snowflake _channelId = new(111);
	private readonly Snowflake _messageId = new(222);

	public MessageClientTests()
	{
		_http.JsonOptions.Returns(new System.Text.Json.JsonSerializerOptions());
		_client = new MessageClient(_http);
	}

	[Fact]
	public async Task CreateAsync_NoFiles_UsesJsonSendAsync()
	{
		_http.SendAsync<Message>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Message());

		var req = new MessageCreateRequest { Content = "hi" };
		await _client.CreateAsync(_channelId, req);

		await _http.Received(1).SendAsync<Message>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}/messages"),
			HttpMethod.Post,
			req,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetAsync_BuildsMessageByIdRouteAsync()
	{
		_http.SendAsync<Message>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Message());

		await _client.GetAsync(_channelId, _messageId);

		await _http.Received(1).SendAsync<Message>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}/messages/{_messageId}"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task EditAsync_PatchesMessageRouteAsync()
	{
		_http.SendAsync<Message>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Message());

		var req = new MessageEditRequest { Content = "edited" };
		await _client.EditAsync(_channelId, _messageId, req);

		await _http.Received(1).SendAsync<Message>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}/messages/{_messageId}"),
			HttpMethod.Patch,
			req,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task DeleteAsync_DeletesMessageRouteAsync()
	{
		await _client.DeleteAsync(_channelId, _messageId);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}/messages/{_messageId}"),
			HttpMethod.Delete,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CrosspostAsync_PostsCrosspostRouteAsync()
	{
		_http.SendAsync<Message>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Message());

		await _client.CrosspostAsync(_channelId, _messageId);

		await _http.Received(1).SendAsync<Message>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}/messages/{_messageId}/crosspost"),
			HttpMethod.Post,
			Arg.Is<object?>(b => b == null),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task AddReactionAsync_PutsReactionMeRoute_UrlEncodedAsync()
	{
		await _client.AddReactionAsync(_channelId, _messageId, "👍");

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString().StartsWith($"channels/{_channelId}/messages/{_messageId}/reactions/") && r.ToString().EndsWith("/@me")),
			HttpMethod.Put,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task RemoveReactionAsync_DeletesReactionMeRouteAsync()
	{
		await _client.RemoveReactionAsync(_channelId, _messageId, "👍");

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString().EndsWith("/@me")),
			HttpMethod.Delete,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task RemoveUserReactionAsync_DeletesReactionWithUserIdAsync()
	{
		var userId = new Snowflake(333);
		await _client.RemoveUserReactionAsync(_channelId, _messageId, "👍", userId);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString().EndsWith($"/{userId}")),
			HttpMethod.Delete,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetReactionsAsync_AppendsAfterAndLimitAsync()
	{
		_http.SendAsync<User[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _client.GetReactionsAsync(_channelId, _messageId, "👍", after: "abc", limit: 10);

		await _http.Received(1).SendAsync<User[]>(
			Arg.Is<DiscordRoute>(r =>
				r.ToString().Contains("/reactions/") &&
				r.ToString().Contains("after=abc") &&
				r.ToString().Contains("limit=10")),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task DeleteAllReactionsAsync_DeletesAllReactionsRouteAsync()
	{
		await _client.DeleteAllReactionsAsync(_channelId, _messageId);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}/messages/{_messageId}/reactions"),
			HttpMethod.Delete,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetMessagesAsync_AppendsAllPaginationQueryParamsAsync()
	{
		_http.SendAsync<Message[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _client.GetMessagesAsync(_channelId, limit: 50, around: new Snowflake(1), before: new Snowflake(2), after: new Snowflake(3));

		await _http.Received(1).SendAsync<Message[]>(
			Arg.Is<DiscordRoute>(r =>
				r.ToString().Contains("limit=50") &&
				r.ToString().Contains("around=1") &&
				r.ToString().Contains("before=2") &&
				r.ToString().Contains("after=3")),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task BulkDeleteMessagesAsync_PostsToBulkDeleteAsync()
	{
		var ids = new[] { new Snowflake(1), new Snowflake(2) };
		await _client.BulkDeleteMessagesAsync(_channelId, ids);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}/messages/bulk-delete"),
			HttpMethod.Post,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task BulkDeleteMessagesAsync_OutsideRange_ThrowsAsync()
	{
		await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _client.BulkDeleteMessagesAsync(_channelId, new[] { new Snowflake(1) }));
	}

	[Fact]
	public async Task TriggerTypingAsync_PostsTypingRouteAsync()
	{
		await _client.TriggerTypingAsync(_channelId);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}/typing"),
			HttpMethod.Post,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task PinAsync_PutsPinRouteAsync()
	{
		await _client.PinAsync(_channelId, _messageId);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}/pins/{_messageId}"),
			HttpMethod.Put,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task UnpinAsync_DeletesPinRouteAsync()
	{
		await _client.UnpinAsync(_channelId, _messageId);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}/pins/{_messageId}"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetPinnedMessagesAsync_GetsPinsRouteAsync()
	{
		_http.SendAsync<Message[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _client.GetPinnedMessagesAsync(_channelId);

		await _http.Received(1).SendAsync<Message[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}/pins"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task EndPollAsync_PostsExpireRouteAsync()
	{
		_http.SendAsync<Message>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Message());

		await _client.EndPollAsync(_channelId, _messageId);

		await _http.Received(1).SendAsync<Message>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}/polls/{_messageId}/expire"),
			HttpMethod.Post,
			Arg.Is<object?>(b => b == null),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetPollVotersAsync_BuildsRouteWithAnswerIdAndQueryAsync()
	{
		_http.SendAsync<User[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _client.GetPollVotersAsync(_channelId, _messageId, answerId: 7, limit: 25);

		await _http.Received(1).SendAsync<User[]>(
			Arg.Is<DiscordRoute>(r =>
				r.ToString().StartsWith($"channels/{_channelId}/polls/{_messageId}/answers/7") &&
				r.ToString().Contains("limit=25")),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}
}

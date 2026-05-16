using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Hosting.Rest.Models;
using DiscoSdk.Models;
using DiscoSdk.Models.Messages;
using DiscoSdk.Hosting.Models.Requests.Messages;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Rest.Clients;

public class WebhookMessageClientTests
{
	private readonly IDiscordRestClient _http = Substitute.For<IDiscordRestClient>();
	private readonly WebhookMessageClient _client;
	private readonly WebhookIdentity _identity = new(new Snowflake(111), "tok");
	private readonly Snowflake _messageId = new(999);

	public WebhookMessageClientTests()
	{
		_http.JsonOptions.Returns(new System.Text.Json.JsonSerializerOptions());
		_client = new WebhookMessageClient(_http);
	}

	[Fact]
	public async Task GetInfoAsync_UsesWebhookIdTokenRouteAsync()
	{
		_http.SendAsync<WebhookInfo>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<CancellationToken>())
			.Returns(new WebhookInfo());

		await _client.GetInfoAsync(_identity);

		await _http.Received(1).SendAsync<WebhookInfo>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"webhooks/{_identity.Id}/{_identity.Token}"),
			HttpMethod.Get,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ExecuteAsync_NoFiles_PostsJsonWithWaitQueryAsync()
	{
		_http.SendAsync<Message?>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Message());

		var req = new ExecuteWebhookRequest { Content = "hi" };
		await _client.ExecuteAsync(_identity, req);

		await _http.Received(1).SendAsync<Message?>(
			Arg.Is<DiscordRoute>(r =>
				r.ToString().StartsWith($"webhooks/{_identity.Id}/{_identity.Token}?") &&
				r.ToString().Contains("wait=true")),
			HttpMethod.Post,
			req,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ExecuteAsync_WithThreadId_AppendsThreadQueryAsync()
	{
		_http.SendAsync<Message?>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Message());

		await _client.ExecuteAsync(_identity, new ExecuteWebhookRequest(), wait: true, threadId: new Snowflake(333));

		await _http.Received(1).SendAsync<Message?>(
			Arg.Is<DiscordRoute>(r => r.ToString().Contains("thread_id=333")),
			HttpMethod.Post,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetAsync_UsesWebhookMessageByIdRouteAsync()
	{
		_http.SendAsync<Message>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Message());

		await _client.GetAsync(_identity, _messageId);

		await _http.Received(1).SendAsync<Message>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"webhooks/{_identity.Id}/{_identity.Token}/messages/{_messageId}"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetAsync_WithThreadId_AppendsThreadQueryAsync()
	{
		_http.SendAsync<Message>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Message());

		await _client.GetAsync(_identity, _messageId, threadId: new Snowflake(444));

		await _http.Received(1).SendAsync<Message>(
			Arg.Is<DiscordRoute>(r => r.ToString().Contains("thread_id=444")),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task EditAsync_PatchesWebhookMessageRouteAsync()
	{
		_http.SendAsync<Message>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Message());

		var req = new MessageEditRequest { Content = "x" };
		await _client.EditAsync(_identity, _messageId, req);

		await _http.Received(1).SendAsync<Message>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"webhooks/{_identity.Id}/{_identity.Token}/messages/{_messageId}"),
			HttpMethod.Patch,
			req,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task DeleteAsync_DeletesWebhookMessageRouteAsync()
	{
		await _client.DeleteAsync(_identity, _messageId);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"webhooks/{_identity.Id}/{_identity.Token}/messages/{_messageId}"),
			HttpMethod.Delete,
			Arg.Is<object?>(b => b == null),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetOriginalResponseAsync_GetsOriginalRouteAsync()
	{
		_http.SendAsync<Message>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Message());

		await _client.GetOriginalResponseAsync(_identity);

		await _http.Received(1).SendAsync<Message>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"webhooks/{_identity.Id}/{_identity.Token}/messages/@original"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task EditOriginalResponseAsync_PatchesOriginalRouteAsync()
	{
		_http.SendAsync<Message>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Message());

		var req = new MessageEditRequest { Content = "x" };
		await _client.EditOriginalResponseAsync(_identity, req);

		await _http.Received(1).SendAsync<Message>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"webhooks/{_identity.Id}/{_identity.Token}/messages/@original"),
			HttpMethod.Patch,
			req,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task DeleteOriginalResponseAsync_DeletesOriginalRouteAsync()
	{
		await _client.DeleteOriginalResponseAsync(_identity);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"webhooks/{_identity.Id}/{_identity.Token}/messages/@original"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task DeleteAsync_MessageDefault_ThrowsAsync()
	{
		await Assert.ThrowsAsync<ArgumentException>(() => _client.DeleteAsync(_identity, default));
	}

	[Fact]
	public async Task ExecuteAsync_NullRequest_ThrowsAsync()
	{
		await Assert.ThrowsAsync<ArgumentNullException>(() => _client.ExecuteAsync(_identity, null!));
	}
}

using DiscoSdk.Exceptions;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Net;

namespace DiscoSdk.Hosting.Tests.Rest.Clients;

/// <summary>
/// Mocks <see cref="IDiscordRestClient"/> and verifies that <see cref="WebhookClient"/> issues the
/// correct route, HTTP verb and body for each operation — no Discord round-trip is performed.
/// </summary>
public class WebhookClientTests
{
	private readonly IDiscordRestClient _http = Substitute.For<IDiscordRestClient>();
	private readonly WebhookClient _client;

	public WebhookClientTests()
	{
		_client = new WebhookClient(_http);
	}

	[Fact]
	public async Task CreateAsync_PostsToChannelWebhooksWithNameAsync()
	{
		var channelId = new Snowflake(111);
		var expected = new Webhook { Id = new Snowflake(999), Name = "hook" };
		_http.SendAsync<Webhook>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/111/webhooks"),
			HttpMethod.Post,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>())
			.Returns(expected);

		var result = await _client.CreateAsync(channelId, "hook");

		Assert.Same(expected, result);
		await _http.Received(1).SendAsync<Webhook>(
			Arg.Is<DiscordRoute>(r => r.Template == "channels/{channel_id}/webhooks" && r.ToString() == "channels/111/webhooks"),
			HttpMethod.Post,
			Arg.Is<object?>(b => b != null),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateAsync_WithAvatar_IncludesAvatarInBodyAsync()
	{
		var channelId = new Snowflake(222);
		_http.SendAsync<Webhook>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Webhook());

		await _client.CreateAsync(channelId, "hook", "data:image/png;base64,abc");

		await _http.Received(1).SendAsync<Webhook>(
			Arg.Any<DiscordRoute>(),
			HttpMethod.Post,
			Arg.Is<object?>(body => HasProperty(body, "avatar", "data:image/png;base64,abc")),
			Arg.Any<CancellationToken>());
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData(null)]
	public async Task CreateAsync_RejectsBlankNameAsync(string? name)
	{
		var channelId = new Snowflake(1);
		await Assert.ThrowsAnyAsync<ArgumentException>(() => _client.CreateAsync(channelId, name!));
	}

	[Fact]
	public async Task GetChannelWebhooksAsync_GetsChannelScopedListAsync()
	{
		var channelId = new Snowflake(555);
		var expected = new[] { new Webhook(), new Webhook() };
		_http.SendAsync<Webhook[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(expected);

		var result = await _client.GetChannelWebhooksAsync(channelId);

		Assert.Same(expected, result);
		await _http.Received(1).SendAsync<Webhook[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/555/webhooks"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetGuildWebhooksAsync_GetsGuildScopedListAsync()
	{
		var guildId = new Snowflake(777);
		_http.SendAsync<Webhook[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _client.GetGuildWebhooksAsync(guildId);

		await _http.Received(1).SendAsync<Webhook[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/777/webhooks"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetAsync_ReturnsWebhookWhenFoundAsync()
	{
		var webhookId = new Snowflake(42);
		var expected = new Webhook { Id = webhookId, Type = WebhookType.Incoming };
		_http.SendAsync<Webhook>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(expected);

		var result = await _client.GetAsync(webhookId);

		Assert.Same(expected, result);
	}

	[Fact]
	public async Task GetAsync_Returns_NullOn404Async()
	{
		_http.SendAsync<Webhook>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.ThrowsAsync(new DiscordApiException(HttpStatusCode.NotFound, "not found", null));

		var result = await _client.GetAsync(new Snowflake(1));

		Assert.Null(result);
	}

	[Fact]
	public async Task GetWithTokenAsync_UsesIdAndTokenRouteAsync()
	{
		var webhookId = new Snowflake(42);
		_http.SendAsync<Webhook>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Webhook { Id = webhookId });

		await _client.GetWithTokenAsync(webhookId, "secret-token");

		await _http.Received(1).SendAsync<Webhook>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "webhooks/42/secret-token"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ModifyAsync_OnlyIncludesSuppliedFieldsAsync()
	{
		_http.SendAsync<Webhook>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Webhook());

		// only name supplied
		await _client.ModifyAsync(new Snowflake(7), name: "new-name");

		await _http.Received(1).SendAsync<Webhook>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "webhooks/7"),
			HttpMethod.Patch,
			Arg.Is<object?>(body => HasDictKey(body, "name") && !HasDictKey(body, "avatar") && !HasDictKey(body, "channel_id")),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task DeleteAsync_UsesWebhookByIdRouteAsync()
	{
		await _client.DeleteAsync(new Snowflake(9));

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "webhooks/9"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task DeleteWithTokenAsync_UsesIdAndTokenRouteAsync()
	{
		await _client.DeleteWithTokenAsync(new Snowflake(9), "tok");

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "webhooks/9/tok"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}

	// ---- helpers ----

	private static bool HasProperty(object? body, string name, object expected)
	{
		if (body is null) return false;
		if (body is IDictionary<string, object?> dict)
			return dict.TryGetValue(name, out var v) && Equals(v, expected);
		var prop = body.GetType().GetProperty(name);
		if (prop is null) return false;
		var actual = prop.GetValue(body);
		return Equals(actual, expected);
	}

	private static bool HasDictKey(object? body, string key)
		=> body is IDictionary<string, object?> dict && dict.ContainsKey(key);
}

using DiscoSdk.Exceptions;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Rest;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Net;

namespace DiscoSdk.Hosting.Tests.Rest.Clients;

public class InviteClientTests
{
	private readonly IDiscordRestClient _http = Substitute.For<IDiscordRestClient>();
	private readonly InviteClient _client;

	public InviteClientTests()
	{
		_client = new InviteClient(_http);
	}

	[Fact]
	public async Task GetAsync_BuildsInvitesByCodeRouteAsync()
	{
		var invite = new Invite { Code = "abc" };
		_http.SendAsync<Invite>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(invite);

		var result = await _client.GetAsync("abc");

		Assert.Same(invite, result);
		await _http.Received(1).SendAsync<Invite>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "invites/abc"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetAsync_WithQueryFlags_AppendsThemAllAsync()
	{
		_http.SendAsync<Invite>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Invite());

		await _client.GetAsync("abc", withCounts: true, withExpiration: true, guildScheduledEventId: new Snowflake(555));

		await _http.Received(1).SendAsync<Invite>(
			Arg.Is<DiscordRoute>(r =>
				r.ToString().StartsWith("invites/abc?") &&
				r.ToString().Contains("with_counts=true") &&
				r.ToString().Contains("with_expiration=true") &&
				r.ToString().Contains("guild_scheduled_event_id=555")),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetAsync_ReturnsNullOn404Async()
	{
		_http.SendAsync<Invite>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.ThrowsAsync(new DiscordApiException(HttpStatusCode.NotFound, "no", null));

		Assert.Null(await _client.GetAsync("nope"));
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public async Task GetAsync_RejectsBlankCodeAsync(string? code)
	{
		await Assert.ThrowsAnyAsync<ArgumentException>(() => _client.GetAsync(code!));
	}

	[Fact]
	public async Task CreateAsync_PostsToChannelInvitesAsync()
	{
		var channelId = new Snowflake(123);
		var request = new { max_age = 0 };
		var invite = new Invite { Code = "x" };
		_http.SendAsync<Invite>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(invite);

		var result = await _client.CreateAsync(channelId, request);

		Assert.Same(invite, result);
		await _http.Received(1).SendAsync<Invite>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/123/invites"),
			HttpMethod.Post,
			request,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetChannelInvitesAsync_ListsInvitesForChannelAsync()
	{
		_http.SendAsync<Invite[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _client.GetChannelInvitesAsync(new Snowflake(42));

		await _http.Received(1).SendAsync<Invite[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/42/invites"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task DeleteAsync_DeletesInviteByCodeAsync()
	{
		await _client.DeleteAsync("xyz");

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "invites/xyz"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}
}

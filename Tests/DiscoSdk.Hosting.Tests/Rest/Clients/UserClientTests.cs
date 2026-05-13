using DiscoSdk.Exceptions;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Models.Applications;
using DiscoSdk.Models.Users;
using DiscoSdk.Rest;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Net;

namespace DiscoSdk.Hosting.Tests.Rest.Clients;

public class UserClientTests
{
	private readonly IDiscordRestClient _http = Substitute.For<IDiscordRestClient>();
	private readonly UserClient _client;

	public UserClientTests()
	{
		_client = new UserClient(_http);
	}

	[Fact]
	public async Task GetAsync_BuildsUsersById_RouteAsync()
	{
		var userId = new Snowflake(42);
		_http.SendAsync<User>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new User());

		await _client.GetAsync(userId);

		await _http.Received(1).SendAsync<User>(
			Arg.Is<DiscordRoute>(r => r.Template == "users/{user_id}" && r.ToString() == "users/42"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetAsync_ReturnsNullOn404Async()
	{
		_http.SendAsync<User>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.ThrowsAsync(new DiscordApiException(HttpStatusCode.NotFound, "no", null));

		Assert.Null(await _client.GetAsync(new Snowflake(1)));
	}

	[Fact]
	public async Task GetAsync_DefaultIdThrowsAsync()
	{
		await Assert.ThrowsAsync<ArgumentException>(() => _client.GetAsync(default));
	}

	[Fact]
	public async Task GetCurrentAsync_UsesMeRouteAsync()
	{
		_http.SendAsync<User>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new User());

		await _client.GetCurrentAsync();

		await _http.Received(1).SendAsync<User>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "users/@me"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ModifyCurrentAsync_PatchesWithSuppliedBodyAsync()
	{
		_http.SendAsync<User>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new User());

		var body = new { username = "Bob" };
		await _client.ModifyCurrentAsync(body);

		await _http.Received(1).SendAsync<User>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "users/@me"),
			HttpMethod.Patch,
			body,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ModifyCurrentAsync_NullBody_ThrowsAsync()
	{
		await Assert.ThrowsAsync<ArgumentNullException>(() => _client.ModifyCurrentAsync(null!));
	}

	[Fact]
	public async Task GetCurrentGuildsAsync_NoArgs_HasNoQueryStringAsync()
	{
		_http.SendAsync<Guild[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _client.GetCurrentGuildsAsync();

		await _http.Received(1).SendAsync<Guild[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "users/@me/guilds"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetCurrentGuildsAsync_BuildsQueryStringAsync()
	{
		_http.SendAsync<Guild[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _client.GetCurrentGuildsAsync(limit: 10, before: new Snowflake(123), after: new Snowflake(456), withCounts: true);

		await _http.Received(1).SendAsync<Guild[]>(
			Arg.Is<DiscordRoute>(r =>
				r.ToString().StartsWith("users/@me/guilds?") &&
				r.ToString().Contains("limit=10") &&
				r.ToString().Contains("before=123") &&
				r.ToString().Contains("after=456") &&
				r.ToString().Contains("with_counts=true")),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetCurrentGuildsAsync_RejectsLimitOutOfRangeAsync()
	{
		await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _client.GetCurrentGuildsAsync(limit: 500));
	}

	[Fact]
	public async Task GetCurrentGuildMemberAsync_GetsMeMemberRouteAsync()
	{
		_http.SendAsync<GuildMember>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new GuildMember());

		await _client.GetCurrentGuildMemberAsync(new Snowflake(777));

		await _http.Received(1).SendAsync<GuildMember>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "users/@me/guilds/777/member"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetConnectionsAsync_GetsConnectionsRouteAsync()
	{
		_http.SendAsync<Connection[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _client.GetConnectionsAsync();

		await _http.Received(1).SendAsync<Connection[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "users/@me/connections"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetApplicationRoleConnectionAsync_BuildsRouteAsync()
	{
		var appId = new Snowflake(123);
		_http.SendAsync<ApplicationRoleConnection>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new ApplicationRoleConnection());

		await _client.GetApplicationRoleConnectionAsync(appId);

		await _http.Received(1).SendAsync<ApplicationRoleConnection>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"users/@me/applications/{appId}/role-connection"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task UpdateApplicationRoleConnectionAsync_PutsBodyAsync()
	{
		var appId = new Snowflake(123);
		var body = new ApplicationRoleConnection { PlatformName = "Twitch" };
		_http.SendAsync<ApplicationRoleConnection>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(body);

		await _client.UpdateApplicationRoleConnectionAsync(appId, body);

		await _http.Received(1).SendAsync<ApplicationRoleConnection>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"users/@me/applications/{appId}/role-connection"),
			HttpMethod.Put,
			body,
			Arg.Any<CancellationToken>());
	}
}

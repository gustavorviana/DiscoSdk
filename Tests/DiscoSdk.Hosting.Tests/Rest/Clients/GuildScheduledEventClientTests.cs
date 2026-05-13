using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Rest.Clients;

public class GuildScheduledEventClientTests
{
	private readonly IDiscordRestClient _http = Substitute.For<IDiscordRestClient>();
	private readonly GuildScheduledEventClient _client;
	private readonly Snowflake _guildId = new(100);
	private readonly Snowflake _eventId = new(800);

	public GuildScheduledEventClientTests()
	{
		_client = new GuildScheduledEventClient(_http);
	}

	[Fact]
	public async Task ListAsync_GetsAsync()
	{
		_http.SendAsync<GuildScheduledEvent[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _client.ListAsync(_guildId, withUserCount: true);

		await _http.Received(1).SendAsync<GuildScheduledEvent[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/scheduled-events?with_user_count=true"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateAsync_PostsAsync()
	{
		_http.SendAsync<GuildScheduledEvent>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new GuildScheduledEvent());

		var req = new { name = "Meetup" };
		await _client.CreateAsync(_guildId, req);

		await _http.Received(1).SendAsync<GuildScheduledEvent>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/scheduled-events"),
			HttpMethod.Post,
			req,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetAsync_GetsByIdAsync()
	{
		_http.SendAsync<GuildScheduledEvent>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new GuildScheduledEvent());

		await _client.GetAsync(_guildId, _eventId);

		await _http.Received(1).SendAsync<GuildScheduledEvent>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/scheduled-events/{_eventId}"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ModifyAsync_PatchesAsync()
	{
		_http.SendAsync<GuildScheduledEvent>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new GuildScheduledEvent());

		var req = new { name = "Renamed" };
		await _client.ModifyAsync(_guildId, _eventId, req);

		await _http.Received(1).SendAsync<GuildScheduledEvent>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/scheduled-events/{_eventId}"),
			HttpMethod.Patch,
			req,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task DeleteAsync_DeletesAsync()
	{
		await _client.DeleteAsync(_guildId, _eventId);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/scheduled-events/{_eventId}"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetUsersAsync_AppliesQueryAsync()
	{
		_http.SendAsync<GuildScheduledEventUser[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _client.GetUsersAsync(_guildId, _eventId, limit: 50, withMember: true, after: new Snowflake(123));

		await _http.Received(1).SendAsync<GuildScheduledEventUser[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/scheduled-events/{_eventId}/users?limit=50&with_member=true&after=123"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}
}

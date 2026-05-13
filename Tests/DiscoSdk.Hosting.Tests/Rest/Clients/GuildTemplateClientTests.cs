using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Rest.Clients;

public class GuildTemplateClientTests
{
	private readonly IDiscordRestClient _http = Substitute.For<IDiscordRestClient>();
	private readonly GuildTemplateClient _client;
	private readonly Snowflake _guildId = new(123);

	public GuildTemplateClientTests()
	{
		_client = new GuildTemplateClient(_http);
	}

	[Fact]
	public async Task GetTemplateAsync_GetsTemplateByCodeAsync()
	{
		_http.SendAsync<GuildTemplate>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new GuildTemplate());

		await _client.GetTemplateAsync("xyz");

		await _http.Received(1).SendAsync<GuildTemplate>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/templates/xyz"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateGuildFromTemplateAsync_PostsToTemplateCodeAsync()
	{
		_http.SendAsync<Guild>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Guild());

		await _client.CreateGuildFromTemplateAsync("code", "MyGuild");

		await _http.Received(1).SendAsync<Guild>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/templates/code"),
			HttpMethod.Post,
			Arg.Is<object?>(body =>
				body!.GetType().GetProperty("name")!.GetValue(body)!.Equals("MyGuild")),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetGuildTemplatesAsync_GetsGuildScopedListAsync()
	{
		_http.SendAsync<GuildTemplate[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _client.GetGuildTemplatesAsync(_guildId);

		await _http.Received(1).SendAsync<GuildTemplate[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/templates"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateGuildTemplateAsync_PostsNameAsync()
	{
		_http.SendAsync<GuildTemplate>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new GuildTemplate());

		await _client.CreateGuildTemplateAsync(_guildId, "tpl-name", "desc");

		await _http.Received(1).SendAsync<GuildTemplate>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/templates"),
			HttpMethod.Post,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task SyncGuildTemplateAsync_PutsTemplateCodeAsync()
	{
		_http.SendAsync<GuildTemplate>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new GuildTemplate());

		await _client.SyncGuildTemplateAsync(_guildId, "code");

		await _http.Received(1).SendAsync<GuildTemplate>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/templates/code"),
			HttpMethod.Put,
			Arg.Is<object?>(b => b == null),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ModifyGuildTemplateAsync_PatchesTemplateCodeAsync()
	{
		_http.SendAsync<GuildTemplate>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new GuildTemplate());

		await _client.ModifyGuildTemplateAsync(_guildId, "code", name: "new");

		await _http.Received(1).SendAsync<GuildTemplate>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/templates/code"),
			HttpMethod.Patch,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task DeleteGuildTemplateAsync_DeletesTemplateCodeAsync()
	{
		_http.SendAsync<GuildTemplate>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new GuildTemplate());

		await _client.DeleteGuildTemplateAsync(_guildId, "code");

		await _http.Received(1).SendAsync<GuildTemplate>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/templates/code"),
			HttpMethod.Delete,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetOnboardingAsync_GetsOnboardingRouteAsync()
	{
		_http.SendAsync<GuildOnboarding>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new GuildOnboarding());

		await _client.GetOnboardingAsync(_guildId);

		await _http.Received(1).SendAsync<GuildOnboarding>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/onboarding"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ModifyOnboardingAsync_PutsOnboardingBodyAsync()
	{
		_http.SendAsync<GuildOnboarding>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new GuildOnboarding());

		var body = new { enabled = true };
		await _client.ModifyOnboardingAsync(_guildId, body);

		await _http.Received(1).SendAsync<GuildOnboarding>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/onboarding"),
			HttpMethod.Put,
			body,
			Arg.Any<CancellationToken>());
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("  ")]
	public async Task GetTemplateAsync_BlankCode_ThrowsAsync(string? code)
	{
		// ArgumentException.ThrowIfNullOrWhiteSpace throws ArgumentNullException for null and
		// ArgumentException for whitespace — both derive from ArgumentException.
		await Assert.ThrowsAnyAsync<ArgumentException>(() => _client.GetTemplateAsync(code!));
	}
}

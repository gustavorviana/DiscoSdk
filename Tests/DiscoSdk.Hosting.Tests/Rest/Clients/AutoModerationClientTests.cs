using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Models.AutoModeration;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Rest.Clients;

public class AutoModerationClientTests
{
	private readonly IDiscordRestClient _http = Substitute.For<IDiscordRestClient>();
	private readonly AutoModerationClient _client;
	private readonly Snowflake _guildId = new(111);
	private readonly Snowflake _ruleId = new(222);

	public AutoModerationClientTests()
	{
		_client = new AutoModerationClient(_http);
	}

	[Fact]
	public async Task ListRulesAsync_GetsRulesRouteAsync()
	{
		_http.SendAsync<AutoModerationRule[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _client.ListRulesAsync(_guildId);

		await _http.Received(1).SendAsync<AutoModerationRule[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/auto-moderation/rules"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetRuleAsync_GetsRuleByIdRouteAsync()
	{
		_http.SendAsync<AutoModerationRule>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new AutoModerationRule());

		await _client.GetRuleAsync(_guildId, _ruleId);

		await _http.Received(1).SendAsync<AutoModerationRule>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/auto-moderation/rules/{_ruleId}"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateRuleAsync_PostsToRulesRouteAsync()
	{
		_http.SendAsync<AutoModerationRule>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new AutoModerationRule());

		var body = new { name = "rule" };
		await _client.CreateRuleAsync(_guildId, body);

		await _http.Received(1).SendAsync<AutoModerationRule>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/auto-moderation/rules"),
			HttpMethod.Post,
			body,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ModifyRuleAsync_PatchesRuleByIdRouteAsync()
	{
		_http.SendAsync<AutoModerationRule>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new AutoModerationRule());

		var body = new { name = "new" };
		await _client.ModifyRuleAsync(_guildId, _ruleId, body);

		await _http.Received(1).SendAsync<AutoModerationRule>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/auto-moderation/rules/{_ruleId}"),
			HttpMethod.Patch,
			body,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task DeleteRuleAsync_DeletesRuleByIdRouteAsync()
	{
		await _client.DeleteRuleAsync(_guildId, _ruleId);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/auto-moderation/rules/{_ruleId}"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ListRulesAsync_DefaultGuildId_ThrowsAsync()
	{
		await Assert.ThrowsAsync<ArgumentException>(() => _client.ListRulesAsync(default));
	}

	[Fact]
	public async Task GetRuleAsync_DefaultRuleId_ThrowsAsync()
	{
		await Assert.ThrowsAsync<ArgumentException>(() => _client.GetRuleAsync(_guildId, default));
	}

	[Fact]
	public async Task CreateRuleAsync_NullBody_ThrowsAsync()
	{
		await Assert.ThrowsAsync<ArgumentNullException>(() => _client.CreateRuleAsync(_guildId, null!));
	}
}

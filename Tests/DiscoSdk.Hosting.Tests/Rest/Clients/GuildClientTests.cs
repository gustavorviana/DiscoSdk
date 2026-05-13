using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using NSubstitute;
using System.Text.Json;

namespace DiscoSdk.Hosting.Tests.Rest.Clients;

public class GuildClientTests
{
	private readonly IDiscordRestClient _http = Substitute.For<IDiscordRestClient>();
	private readonly GuildClient _client;
	private readonly Snowflake _guildId = new(111);
	private readonly Snowflake _userId = new(222);

	public GuildClientTests()
	{
		_client = new GuildClient(_http);
	}

	[Fact]
	public async Task GetAsync_GetsGuildByIdAsync()
	{
		_http.SendAsync<Guild>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Guild());

		await _client.GetAsync(_guildId);

		await _http.Received(1).SendAsync<Guild>(
			Arg.Is<DiscordRoute>(r => r.Template == "guilds/{guild_id}" && r.ToString() == $"guilds/{_guildId}"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetMembersAsync_AppendsLimitAndAfterAsQueryAsync()
	{
		_http.SendAsync<GuildMember[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _client.GetMembersAsync(_guildId, limit: 50, after: new Snowflake(999));

		await _http.Received(1).SendAsync<GuildMember[]>(
			Arg.Is<DiscordRoute>(r =>
				r.ToString().StartsWith($"guilds/{_guildId}/members?") &&
				r.ToString().Contains("limit=50") &&
				r.ToString().Contains("after=999")),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetMembersAsync_LimitOutOfRange_ThrowsAsync()
	{
		await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _client.GetMembersAsync(_guildId, limit: 5000));
	}

	[Fact]
	public async Task GetMemberAsync_FetchesSingleMemberAsync()
	{
		_http.SendAsync<GuildMember>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new GuildMember());

		await _client.GetMemberAsync(_guildId, _userId);

		await _http.Received(1).SendAsync<GuildMember>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/members/{_userId}"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateChannelAsync_PostsToGuildChannelsAsync()
	{
		_http.SendAsync<Channel>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Channel());

		var req = new { name = "general", type = 0 };
		await _client.CreateChannelAsync(_guildId, req);

		await _http.Received(1).SendAsync<Channel>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/channels"),
			HttpMethod.Post,
			req,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task BanMemberAsync_PutsToBanRouteAsync()
	{
		await _client.BanMemberAsync(_guildId, _userId, new { delete_message_seconds = 0 });

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/bans/{_userId}"),
			HttpMethod.Put,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task KickMemberAsync_DeletesMemberRouteAsync()
	{
		await _client.KickMemberAsync(_guildId, _userId);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/members/{_userId}"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task DeleteAsync_UsesGuildRootAsync()
	{
		await _client.DeleteAsync(_guildId);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task LeaveAsync_UsesUserGuildsRouteAsync()
	{
		await _client.LeaveAsync(_guildId);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"users/@me/guilds/{_guildId}"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}

	// ---- New endpoints (positions, bulk-ban, OAuth, member roles, MFA, integrations, incidents, search, bans pagination) ----

	[Fact]
	public async Task ModifyChannelPositionsAsync_PatchesChannelsListAsync()
	{
		await _client.ModifyChannelPositionsAsync(_guildId, new[] { new { id = "1", position = 0 } });

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/channels"),
			HttpMethod.Patch,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetBansAsync_AppendsPaginationQueryAsync()
	{
		_http.SendAsync<Ban[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _client.GetBansAsync(_guildId, limit: 100, before: new Snowflake(7), after: new Snowflake(9));

		await _http.Received(1).SendAsync<Ban[]>(
			Arg.Is<DiscordRoute>(r =>
				r.ToString().StartsWith($"guilds/{_guildId}/bans?") &&
				r.ToString().Contains("limit=100") &&
				r.ToString().Contains("before=7") &&
				r.ToString().Contains("after=9")),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task BulkBanAsync_PostsBulkBanRouteAsync()
	{
		_http.SendAsync<JsonElement>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(JsonDocument.Parse("""{"banned_users":["1","2"]}""").RootElement);

		await _client.BulkBanAsync(_guildId, new[] { new Snowflake(1), new Snowflake(2) }, deleteMessageSeconds: 60);

		await _http.Received(1).SendAsync<JsonElement>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/bulk-ban"),
			HttpMethod.Post,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task AddMemberAsync_PutsMemberWithAccessTokenAsync()
	{
		_http.SendAsync<GuildMember>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new GuildMember());

		await _client.AddMemberAsync(_guildId, _userId, "tok", nick: "Bob");

		await _http.Received(1).SendAsync<GuildMember>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/members/{_userId}"),
			HttpMethod.Put,
			Arg.Is<object?>(body => DictHasValue(body, "access_token", "tok")),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task AddMemberAsync_BlankToken_ThrowsAsync()
	{
		await Assert.ThrowsAsync<ArgumentException>(() => _client.AddMemberAsync(_guildId, _userId, ""));
	}

	[Fact]
	public async Task ModifyCurrentMemberAsync_PatchesMeMemberAsync()
	{
		_http.SendAsync<GuildMember>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new GuildMember());

		await _client.ModifyCurrentMemberAsync(_guildId, "new-nick");

		await _http.Received(1).SendAsync<GuildMember>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/members/@me"),
			HttpMethod.Patch,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ModifyMemberAsync_PatchesMemberWithBodyAsync()
	{
		var body = new { nick = "new" };
		_http.SendAsync<GuildMember>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new GuildMember());

		await _client.ModifyMemberAsync(_guildId, _userId, body);

		await _http.Received(1).SendAsync<GuildMember>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/members/{_userId}"),
			HttpMethod.Patch,
			body,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task AddMemberRoleAsync_PutsMemberRoleRouteAsync()
	{
		var roleId = new Snowflake(333);
		await _client.AddMemberRoleAsync(_guildId, _userId, roleId);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/members/{_userId}/roles/{roleId}"),
			HttpMethod.Put,
			Arg.Is<object?>(b => b == null),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task RemoveMemberRoleAsync_DeletesMemberRoleRouteAsync()
	{
		var roleId = new Snowflake(333);
		await _client.RemoveMemberRoleAsync(_guildId, _userId, roleId);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/members/{_userId}/roles/{roleId}"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ModifyMfaLevelAsync_PostsMfaRouteWithIntLevelAsync()
	{
		await _client.ModifyMfaLevelAsync(_guildId, MfaLevel.Elevated);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/mfa"),
			HttpMethod.Post,
			Arg.Is<object?>(body => body!.GetType().GetProperty("level")!.GetValue(body)!.Equals((int)MfaLevel.Elevated)),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ListIntegrationsAsync_GetsIntegrationsRouteAsync()
	{
		_http.SendAsync<Integration[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _client.ListIntegrationsAsync(_guildId);

		await _http.Received(1).SendAsync<Integration[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/integrations"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task DeleteIntegrationAsync_DeletesIntegrationByIdAsync()
	{
		var integrationId = new Snowflake(444);
		await _client.DeleteIntegrationAsync(_guildId, integrationId);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/integrations/{integrationId}"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ModifyIncidentActionsAsync_PutsIncidentActionsRouteAsync()
	{
		_http.SendAsync<IncidentsData>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new IncidentsData());

		await _client.ModifyIncidentActionsAsync(_guildId, DateTimeOffset.UtcNow.AddHours(1), null);

		await _http.Received(1).SendAsync<IncidentsData>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/incident-actions"),
			HttpMethod.Put,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task SearchMembersAsync_BuildsSearchQueryAsync()
	{
		_http.SendAsync<GuildMember[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _client.SearchMembersAsync(_guildId, "ali", limit: 10);

		await _http.Received(1).SendAsync<GuildMember[]>(
			Arg.Is<DiscordRoute>(r =>
				r.ToString().StartsWith($"guilds/{_guildId}/members/search?") &&
				r.ToString().Contains("query=ali") &&
				r.ToString().Contains("limit=10")),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task SearchMembersAsync_BlankQuery_ThrowsAsync()
	{
		await Assert.ThrowsAsync<ArgumentException>(() => _client.SearchMembersAsync(_guildId, ""));
	}

	[Fact]
	public async Task GetVanityUrlAsync_GetsVanityUrlRouteAsync()
	{
		_http.SendAsync<VanityUrl?>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<CancellationToken>())
			.Returns((VanityUrl?)null);

		await _client.GetVanityUrlAsync(_guildId);

		await _http.Received(1).SendAsync<VanityUrl?>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/vanity-url"),
			HttpMethod.Get,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetAuditLogsAsync_AppendsFiltersAsync()
	{
		_http.SendAsync<AuditLog>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new AuditLog());

		await _client.GetAuditLogsAsync(_guildId, limit: 5, before: new Snowflake(1), userId: new Snowflake(2), actionType: AuditLogActionType.GuildUpdate);

		await _http.Received(1).SendAsync<AuditLog>(
			Arg.Is<DiscordRoute>(r =>
				r.ToString().StartsWith($"guilds/{_guildId}/audit-logs?") &&
				r.ToString().Contains("limit=5") &&
				r.ToString().Contains("before=1") &&
				r.ToString().Contains("user_id=2") &&
				r.ToString().Contains($"action_type={(int)AuditLogActionType.GuildUpdate}")),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	private static bool DictHasValue(object? body, string key, object? expected)
	{
		if (body is not IDictionary<string, object?> dict) return false;
		return dict.TryGetValue(key, out var v) && Equals(v, expected);
	}

	[Fact]
	public async Task RequestToSpeakAsync_PatchesMeVoiceStateWithChannelAsync()
	{
		var channelId = new Snowflake(555);
		await _client.RequestToSpeakAsync(_guildId, channelId);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/voice-states/@me"),
			HttpMethod.Patch,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}
}

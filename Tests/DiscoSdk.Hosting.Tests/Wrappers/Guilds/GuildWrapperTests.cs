using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.AutoModeration;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Users;
using DiscoSdk.Rest;
using NSubstitute;
using System.Text.Json;

namespace DiscoSdk.Hosting.Tests.Wrappers.Guilds;

/// <summary>
/// Verifies that every action method on <see cref="GuildWrapper"/> emits the right Discord
/// route / verb / body on the mocked <see cref="IDiscordRestClient"/>.
/// </summary>
public class GuildWrapperTests : WrapperTestBase
{
	private readonly Snowflake _guildId = new(100);
	private readonly GuildWrapper _wrapper;

	public GuildWrapperTests()
	{
		_wrapper = new GuildWrapper(new DiscoSdk.Models.Guild { Id = _guildId, Name = "g" }, Client);
	}

	// ---- Lifecycle ----

	[Fact]
	public async Task Delete_DeletesGuildAsync()
	{
		await _wrapper.Delete().ExecuteAsync();
		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100"),
			HttpMethod.Delete, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Leave_DeletesMeGuildsRouteAsync()
	{
		await _wrapper.Leave().ExecuteAsync();
		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "users/@me/guilds/100"),
			HttpMethod.Delete, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Edit_PatchesGuildWithChangesAsync()
	{
		Http.SendAsync<DiscoSdk.Models.Guild>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new DiscoSdk.Models.Guild { Id = _guildId });

		await _wrapper.Edit().SetName("new").ExecuteAsync();

		await Http.Received(1).SendAsync<DiscoSdk.Models.Guild>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100"),
			HttpMethod.Patch,
			Arg.Is<object?>(b => BodyContains(b, "name", "new")),
			Arg.Any<CancellationToken>());
	}

	// ---- Members ----

	[Fact]
	public async Task UnbanMember_DeletesBanAsync()
	{
		await _wrapper.UnbanMember(new Snowflake(42)).ExecuteAsync();
		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/bans/42"),
			HttpMethod.Delete, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task KickMember_DeletesMemberAsync()
	{
		await _wrapper.KickMember(new Snowflake(42)).ExecuteAsync();
		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/members/42"),
			HttpMethod.Delete, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task BanMember_PutsBanRouteWithDeleteDaysAsync()
	{
		await _wrapper.BanMember(new Snowflake(42), deleteMessageDays: 3).ExecuteAsync();
		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/bans/42"),
			HttpMethod.Put,
			Arg.Is<object?>(b => BodyContains(b, "delete_message_days", 3)),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetMember_FetchesMemberByIdAsync()
	{
		Http.SendAsync<GuildMember>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new GuildMember { User = new User { UserId = new Snowflake(42), Username = "u" } });

		await _wrapper.GetMember(new Snowflake(42)).ExecuteAsync();

		await Http.Received(1).SendAsync<GuildMember>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/members/42"),
			HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetBan_FetchesBanByIdAsync()
	{
		Http.SendAsync<Ban>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Ban { User = new User { UserId = new Snowflake(42), Username = "u" } });

		await _wrapper.GetBan(new Snowflake(42)).ExecuteAsync();

		await Http.Received(1).SendAsync<Ban>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/bans/42"),
			HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetMembers_PaginationAction_ListsMembersAsync()
	{
		Http.SendAsync<GuildMember[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _wrapper.GetMembers().ExecuteAsync();

		await Http.Received(1).SendAsync<GuildMember[]>(
			Arg.Is<DiscordRoute>(r => r.ToString().StartsWith("guilds/100/members")),
			HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetBans_PaginationAction_ListsBansAsync()
	{
		Http.SendAsync<Ban[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _wrapper.GetBans().ExecuteAsync();

		await Http.Received(1).SendAsync<Ban[]>(
			Arg.Is<DiscordRoute>(r => r.ToString().StartsWith("guilds/100/bans")),
			HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task BulkBan_PostsBulkBanRouteAsync()
	{
		Http.SendAsync<JsonElement>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(JsonDocument.Parse("""{"banned_users":["1","2"]}""").RootElement);

		await _wrapper.BulkBan([new Snowflake(1), new Snowflake(2)], deleteMessageSeconds: 60).ExecuteAsync();

		await Http.Received(1).SendAsync<JsonElement>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/bulk-ban"),
			HttpMethod.Post, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task SearchMembers_GetsSearchRouteWithQueryAsync()
	{
		Http.SendAsync<GuildMember[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _wrapper.SearchMembers("alice", limit: 10).ExecuteAsync();

		await Http.Received(1).SendAsync<GuildMember[]>(
			Arg.Is<DiscordRoute>(r =>
				r.ToString().StartsWith("guilds/100/members/search?") &&
				r.ToString().Contains("query=alice") &&
				r.ToString().Contains("limit=10")),
			HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task AddMember_PutsMemberWithAccessTokenAsync()
	{
		Http.SendAsync<GuildMember>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new GuildMember { User = new User { UserId = new Snowflake(42), Username = "u" } });

		await _wrapper.AddMember(new Snowflake(42), "tok", nick: "Bob").ExecuteAsync();

		await Http.Received(1).SendAsync<GuildMember>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/members/42"),
			HttpMethod.Put,
			Arg.Is<object?>(b => BodyContains(b, "access_token", "tok") && BodyContains(b, "nick", "Bob")),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ModifyCurrentMember_PatchesMeMemberAsync()
	{
		Http.SendAsync<GuildMember>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new GuildMember { User = new User { UserId = new Snowflake(1), Username = "bot" } });

		await _wrapper.ModifyCurrentMember("new-nick").ExecuteAsync();

		await Http.Received(1).SendAsync<GuildMember>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/members/@me"),
			HttpMethod.Patch, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ModifyMember_PatchesMemberWithFieldsAsync()
	{
		Http.SendAsync<GuildMember>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new GuildMember { User = new User { UserId = new Snowflake(42), Username = "u" } });

		await _wrapper.ModifyMember(new Snowflake(42), nick: "new", mute: true).ExecuteAsync();

		await Http.Received(1).SendAsync<GuildMember>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/members/42"),
			HttpMethod.Patch,
			Arg.Is<object?>(b => BodyContains(b, "nick", "new") && BodyContains(b, "mute", true)),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task AddMemberRole_PutsMemberRoleAsync()
	{
		await _wrapper.AddMemberRole(new Snowflake(42), new Snowflake(7)).ExecuteAsync();
		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/members/42/roles/7"),
			HttpMethod.Put, Arg.Is<object?>(b => b == null), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task RemoveMemberRole_DeletesMemberRoleAsync()
	{
		await _wrapper.RemoveMemberRole(new Snowflake(42), new Snowflake(7)).ExecuteAsync();
		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/members/42/roles/7"),
			HttpMethod.Delete, Arg.Any<CancellationToken>());
	}

	// ---- Channels / roles / emojis ----

	[Fact]
	public async Task CreateChannel_PostsGuildChannelsRouteAsync()
	{
		Http.SendAsync<Channel>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Channel { Id = new Snowflake(500), Type = ChannelType.GuildText });

		await _wrapper.CreateChannel("general", ChannelType.GuildText).ExecuteAsync();

		await Http.Received(1).SendAsync<Channel>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/channels"),
			HttpMethod.Post,
			Arg.Is<object?>(b => BodyContains(b, "name", "general") && BodyContains(b, "type", (int)ChannelType.GuildText)),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateRole_PostsGuildRolesRouteAsync()
	{
		Http.SendAsync<Role>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Role());

		await _wrapper.CreateRole().SetName("Admin").ExecuteAsync();

		await Http.Received(1).SendAsync<Role>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/roles"),
			HttpMethod.Post,
			Arg.Is<object?>(b => BodyContains(b, "name", "Admin")),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateEmoji_PostsGuildEmojisRouteAsync()
	{
		Http.SendAsync<Emoji>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Emoji { Id = new Snowflake(11), Name = "smile" });

		await _wrapper.CreateEmoji("smile", new DiscordImageBuffer([1, 2, 3], "png")).ExecuteAsync();

		await Http.Received(1).SendAsync<Emoji>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/emojis"),
			HttpMethod.Post,
			Arg.Is<object?>(b => BodyContains(b, "name", "smile")),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ModifyChannelPositions_PatchesChannelsAsync()
	{
		var positions = new[] { new ChannelPosition(new Snowflake(1), Position: 0) };

		await _wrapper.ModifyChannelPositions(positions).ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/channels"),
			HttpMethod.Patch, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetRoles_GetsRolesRouteAsync()
	{
		Http.SendAsync<Role[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _wrapper.GetRoles().ExecuteAsync();

		await Http.Received(1).SendAsync<Role[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/roles"),
			HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetInvites_GetsInvitesRouteAsync()
	{
		Http.SendAsync<Invite[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _wrapper.GetInvites().ExecuteAsync();

		await Http.Received(1).SendAsync<Invite[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/invites"),
			HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	// ---- Audit logs / prune / regions / preview / widget / welcome / vanity ----

	[Fact]
	public async Task GetAuditLogs_GetsAuditLogsRouteAsync()
	{
		Http.SendAsync<AuditLog>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new AuditLog());

		await _wrapper.GetAuditLogs().ExecuteAsync();

		await Http.Received(1).SendAsync<AuditLog>(
			Arg.Is<DiscordRoute>(r => r.ToString().StartsWith("guilds/100/audit-logs")),
			HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetPruneCount_GetsPruneRouteAsync()
	{
		Http.SendAsync<Dictionary<string, object>>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Dictionary<string, object> { ["pruned"] = 5 });

		await _wrapper.GetPruneCount(days: 7).ExecuteAsync();

		await Http.Received(1).SendAsync<Dictionary<string, object>>(
			Arg.Is<DiscordRoute>(r => r.ToString().StartsWith("guilds/100/prune") && r.ToString().Contains("days=7")),
			HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task BeginPrune_PostsPruneRouteAsync()
	{
		Http.SendAsync<Dictionary<string, object>>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Dictionary<string, object> { ["pruned"] = 5 });

		await _wrapper.BeginPrune(days: 7).ExecuteAsync();

		await Http.Received(1).SendAsync<Dictionary<string, object>>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/prune"),
			HttpMethod.Post, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetVoiceRegions_GetsRegionsRouteAsync()
	{
		Http.SendAsync<VoiceRegion[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _wrapper.GetVoiceRegions().ExecuteAsync();

		await Http.Received(1).SendAsync<VoiceRegion[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/regions"),
			HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetPreview_GetsPreviewRouteAsync()
	{
		Http.SendAsync<GuildPreview>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new GuildPreview());

		await _wrapper.GetPreview().ExecuteAsync();

		await Http.Received(1).SendAsync<GuildPreview>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/preview"),
			HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetWidget_GetsWidgetJsonRouteAsync()
	{
		Http.SendAsync<GuildWidget>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new GuildWidget());

		await _wrapper.GetWidget().ExecuteAsync();

		await Http.Received(1).SendAsync<GuildWidget>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/widget.json"),
			HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task EditWidget_PatchesWidgetRouteAsync()
	{
		Http.SendAsync<GuildWidget>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new GuildWidget());

		await _wrapper.EditWidget().SetEnabled(true).ExecuteAsync();

		await Http.Received(1).SendAsync<GuildWidget>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/widget"),
			HttpMethod.Patch, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetWelcomeScreen_GetsWelcomeScreenRouteAsync()
	{
		Http.SendAsync<WelcomeScreen>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new WelcomeScreen());

		await _wrapper.GetWelcomeScreen().ExecuteAsync();

		await Http.Received(1).SendAsync<WelcomeScreen>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/welcome-screen"),
			HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task EditWelcomeScreen_PatchesWelcomeScreenRouteAsync()
	{
		Http.SendAsync<WelcomeScreen>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new WelcomeScreen());

		await _wrapper.EditWelcomeScreen().SetEnabled(true).ExecuteAsync();

		await Http.Received(1).SendAsync<WelcomeScreen>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/welcome-screen"),
			HttpMethod.Patch, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetVanityUrl_GetsVanityUrlRouteAsync()
	{
		Http.SendAsync<VanityUrl?>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<CancellationToken>())
			.Returns((VanityUrl?)null);

		await _wrapper.GetVanityUrl().ExecuteAsync();

		await Http.Received(1).SendAsync<VanityUrl?>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/vanity-url"),
			HttpMethod.Get, Arg.Any<CancellationToken>());
	}

	// ---- Auto-moderation ----

	[Fact]
	public async Task GetAutoModerationRules_GetsRulesRouteAsync()
	{
		Http.SendAsync<AutoModerationRule[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _wrapper.GetAutoModerationRules().ExecuteAsync();

		await Http.Received(1).SendAsync<AutoModerationRule[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/auto-moderation/rules"),
			HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetAutoModerationRule_GetsRuleByIdRouteAsync()
	{
		Http.SendAsync<AutoModerationRule>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new AutoModerationRule { GuildId = _guildId, Actions = [], ExemptRoles = [], ExemptChannels = [] });

		await _wrapper.GetAutoModerationRule(new Snowflake(99)).ExecuteAsync();

		await Http.Received(1).SendAsync<AutoModerationRule>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/auto-moderation/rules/99"),
			HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateAutoModerationRule_PostsRuleAsync()
	{
		Http.SendAsync<AutoModerationRule>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new AutoModerationRule { GuildId = _guildId, Actions = [], ExemptRoles = [], ExemptChannels = [] });

		await _wrapper.CreateAutoModerationRule("rule", AutoModerationEventType.MessageSend, AutoModerationTriggerType.Keyword)
			.ExecuteAsync();

		await Http.Received(1).SendAsync<AutoModerationRule>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/auto-moderation/rules"),
			HttpMethod.Post,
			Arg.Is<object?>(b => BodyContains(b, "name", "rule")),
			Arg.Any<CancellationToken>());
	}

	// ---- Templates / Onboarding ----

	[Fact]
	public async Task GetOnboarding_GetsOnboardingRouteAsync()
	{
		Http.SendAsync<GuildOnboarding>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new GuildOnboarding { GuildId = _guildId });

		await _wrapper.GetOnboarding().ExecuteAsync();

		await Http.Received(1).SendAsync<GuildOnboarding>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/onboarding"),
			HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetTemplates_GetsTemplatesListRouteAsync()
	{
		Http.SendAsync<GuildTemplate[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _wrapper.GetTemplates().ExecuteAsync();

		await Http.Received(1).SendAsync<GuildTemplate[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/templates"),
			HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateTemplate_PostsTemplateAsync()
	{
		Http.SendAsync<GuildTemplate>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new GuildTemplate { Code = "x", Creator = new User() });

		await _wrapper.CreateTemplate("tpl", "desc").ExecuteAsync();

		await Http.Received(1).SendAsync<GuildTemplate>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/templates"),
			HttpMethod.Post,
			Arg.Is<object?>(b => BodyPropertyEquals(b, "name", "tpl") && BodyPropertyEquals(b, "description", "desc")),
			Arg.Any<CancellationToken>());
	}

	// ---- MFA / Integrations / Incidents / Webhooks ----

	[Fact]
	public async Task ModifyMfaLevel_PostsMfaRouteAsync()
	{
		await _wrapper.ModifyMfaLevel(MfaLevel.Elevated).ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/mfa"),
			HttpMethod.Post,
			Arg.Is<object?>(body => BodyPropertyEquals(body, "level", (int)MfaLevel.Elevated)),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetIntegrations_GetsIntegrationsRouteAsync()
	{
		Http.SendAsync<Integration[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _wrapper.GetIntegrations().ExecuteAsync();

		await Http.Received(1).SendAsync<Integration[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/integrations"),
			HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ModifyIncidentActions_PutsIncidentActionsAsync()
	{
		Http.SendAsync<IncidentsData>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new IncidentsData());

		await _wrapper.ModifyIncidentActions(DateTimeOffset.UtcNow.AddHours(1), null).ExecuteAsync();

		await Http.Received(1).SendAsync<IncidentsData>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/incident-actions"),
			HttpMethod.Put, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetWebhooks_GetsGuildWebhooksRouteAsync()
	{
		Http.SendAsync<Webhook[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _wrapper.GetWebhooks().ExecuteAsync();

		await Http.Received(1).SendAsync<Webhook[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/webhooks"),
			HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}
}

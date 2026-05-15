using DiscoSdk;
using DiscoSdk.Exceptions;
using DiscoSdk.Hosting.Builders;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System.Text.Json;

namespace DiscoSdk.Hosting.Tests.Rest.Actions;

/// <summary>
/// Covers intent pre-checks added via <see cref="IntentGuard"/> so the SDK fails fast with
/// <see cref="MissingIntentException"/> instead of letting Discord reject the request (or
/// silently return empty data).
/// </summary>
public class IntentGuardedActionsTests
{
	private readonly Snowflake _guildId = new(100);

	private DiscordClient BuildClient(DiscordIntent intents, FakeGatewaySocket? socket = null)
	{
		var http = Substitute.For<IDiscordRestClient>();
		http.JsonOptions.Returns(new JsonSerializerOptions());

		var builder = DiscordClientBuilder.Create("test-token")
			.WithIntents(intents)
			.WithLogger(NullLogger.Instance)
			.WithRestClient(http);

		if (socket != null)
			builder = builder.WithGatewaySocketFactory(new FakeGatewaySocketFactory(socket));

		var client = builder.Build();
		if (socket != null)
			client.SeedShardsForTests(totalShards: 1);
		return client;
	}

	[Fact]
	public async Task MemberPagination_WithoutGuildMembersIntent_ThrowsAsync()
	{
		var client = BuildClient(DiscordIntent.Guilds);
		var guild = Substitute.For<IGuild>();
		guild.Id.Returns(_guildId);
		var action = new MemberPaginationAction(client, guild);

		var ex = await Assert.ThrowsAsync<MissingIntentException>(() => action.ExecuteAsync());
		Assert.Equal(DiscordIntent.GuildMembers, ex.RequiredIntent);
		Assert.Contains("list guild members", ex.Message);
	}

	[Fact]
	public async Task RequestGuildMembers_EmptyQueryWithoutGuildMembersIntent_ThrowsAsync()
	{
		var socket = new FakeGatewaySocket();
		var client = BuildClient(DiscordIntent.Guilds, socket);
		var action = new RequestGuildMembersAction(client, _guildId);

		var ex = await Assert.ThrowsAsync<MissingIntentException>(() => action.GetAsync());
		Assert.Equal(DiscordIntent.GuildMembers, ex.RequiredIntent);
		Assert.Contains("full guild member list", ex.Message);

		Assert.Empty(socket.SentFrames);
	}

	[Fact]
	public async Task RequestGuildMembers_PresencesTrueWithoutGuildPresencesIntent_ThrowsAsync()
	{
		var socket = new FakeGatewaySocket();
		var client = BuildClient(DiscordIntent.Guilds | DiscordIntent.GuildMembers, socket);
		var action = new RequestGuildMembersAction(client, _guildId).SetPresences(true);

		var ex = await Assert.ThrowsAsync<MissingIntentException>(() => action.GetAsync());
		Assert.Equal(DiscordIntent.GuildPresences, ex.RequiredIntent);
		Assert.Contains("presences", ex.Message);

		Assert.Empty(socket.SentFrames);
	}

	[Fact]
	public async Task RequestGuildMembers_NonEmptyQueryWithoutGuildMembersIntent_DoesNotThrowAsync()
	{
		var socket = new FakeGatewaySocket();
		var client = BuildClient(DiscordIntent.Guilds, socket);
		var action = new RequestGuildMembersAction(client, _guildId).SetQuery("alice").SetLimit(5);

		// Should send op-8 without throwing — partial query doesn't need GUILD_MEMBERS.
		var task = action.GetAsync();
		var deadline = DateTime.UtcNow.AddSeconds(2);
		while (socket.SentFrames.Count == 0 && DateTime.UtcNow < deadline)
			await Task.Delay(10);

		Assert.Single(socket.SentFrames);

		// Cancel the pending request — coordinator drains, GetAsync completes with empty/canceled.
		using var cts = new CancellationTokenSource();
		cts.Cancel();
		try { await task.WaitAsync(TimeSpan.FromMilliseconds(50)); } catch { /* expected */ }
	}

	[Fact]
	public async Task RequestGuildMembers_UserIdsWithoutGuildMembersIntent_DoesNotThrowAsync()
	{
		var socket = new FakeGatewaySocket();
		var client = BuildClient(DiscordIntent.Guilds, socket);
		var action = new RequestGuildMembersAction(client, _guildId).SetUserIds(new Snowflake(42));

		var task = action.GetAsync();
		var deadline = DateTime.UtcNow.AddSeconds(2);
		while (socket.SentFrames.Count == 0 && DateTime.UtcNow < deadline)
			await Task.Delay(10);

		Assert.Single(socket.SentFrames);

		try { await task.WaitAsync(TimeSpan.FromMilliseconds(50)); } catch { /* expected */ }
	}
}

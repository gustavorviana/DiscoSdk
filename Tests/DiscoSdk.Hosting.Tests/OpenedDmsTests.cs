using DiscoSdk;
using DiscoSdk.Hosting.Builders;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Users;
using DiscoSdk.Rest;
using NSubstitute;
using System.Text.Json;

namespace DiscoSdk.Hosting.Tests;

/// <summary>
/// Behavioural tests for <see cref="IDiscordClient.OpenedDms"/> — the in-memory snapshot of
/// 1-to-1 DM channels opened during the current session.
/// </summary>
public class OpenedDmsTests
{
	private readonly IDiscordRestClient _http = Substitute.For<IDiscordRestClient>();
	private readonly DiscordClient _client;

	public OpenedDmsTests()
	{
		_http.JsonOptions.Returns(new JsonSerializerOptions());

		_client = DiscordClientBuilder.Create("test-token")
			.WithIntents(DiscordIntent.None)
			.WithRestClient(_http)
			.Build();

		// DiscordClient.OpenDm does a user-existence check before opening the DM.
		// Always return a non-null user so the check passes for every snowflake we hand in.
		_http.SendAsync<User>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new User { UserId = new Snowflake(1), Username = "stub" });
	}

	private static Channel DmChannel(ulong channelId) => new()
	{
		Id = new Snowflake(channelId),
		Type = ChannelType.Dm,
	};

	[Fact]
	public void OpenedDms_OnFreshClient_IsEmpty()
	{
		Assert.Empty(_client.OpenedDms);
	}

	[Fact]
	public async Task OpenedDms_AfterOpenDm_ContainsTheOpenedChannelAsync()
	{
		_http.SendAsync<Channel>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(DmChannel(channelId: 500));

		var dm = await _client.OpenDm(new Snowflake(42)).ExecuteAsync();

		Assert.Single(_client.OpenedDms);
		Assert.Equal(dm.Id, _client.OpenedDms[0].Id);
	}

	[Fact]
	public async Task OpenedDms_MultipleOpens_TracksEachUserOnceAsync()
	{
		_http.SendAsync<Channel>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(_ => DmChannel(channelId: Random.Shared.NextInt64() switch { var x => (ulong)x }));

		await _client.OpenDm(new Snowflake(1)).ExecuteAsync();
		await _client.OpenDm(new Snowflake(2)).ExecuteAsync();
		await _client.OpenDm(new Snowflake(3)).ExecuteAsync();

		// Re-opening the same user is a cache hit — count must not grow.
		await _client.OpenDm(new Snowflake(2)).ExecuteAsync();

		Assert.Equal(3, _client.OpenedDms.Count);
	}

	[Fact]
	public async Task OpenedDms_ReturnsSnapshot_NotLiveViewAsync()
	{
		_http.SendAsync<Channel>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(_ => DmChannel(channelId: Random.Shared.NextInt64() switch { var x => (ulong)x }));

		await _client.OpenDm(new Snowflake(1)).ExecuteAsync();
		var snapshot = _client.OpenedDms;

		// Opening another DM after taking the snapshot must NOT mutate the snapshot.
		await _client.OpenDm(new Snowflake(2)).ExecuteAsync();

		Assert.Single(snapshot);
		Assert.Equal(2, _client.OpenedDms.Count);
	}

	[Fact]
	public async Task OpenedDms_DoesNotTrackGroupDmsAsync()
	{
		// CreateGroupDm goes through a different code path (not memoized by user id).
		_http.SendAsync<Channel>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Channel { Id = new Snowflake(999), Type = ChannelType.GroupDm });

		await _client.CreateGroupDm()
			.AddRecipient("token-a")
			.AddRecipient("token-b")
			.ExecuteAsync();

		Assert.Empty(_client.OpenedDms);
	}
}

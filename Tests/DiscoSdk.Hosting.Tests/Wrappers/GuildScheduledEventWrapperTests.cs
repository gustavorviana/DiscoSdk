using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers;

/// <summary>
/// Verifies the public actions on <see cref="GuildScheduledEventWrapper"/> hit the right Discord
/// REST routes — Http mock captures the calls, no Discord round-trip.
/// </summary>
public class GuildScheduledEventWrapperTests : WrapperTestBase
{
	private readonly Snowflake _guildId = new(100);
	private readonly Snowflake _eventId = new(800);

	private GuildScheduledEventWrapper NewWrapper(string name = "Meetup") =>
		new(Client, new GuildScheduledEvent
		{
			Id = _eventId,
			GuildId = _guildId,
			Name = name,
			ScheduledStartTime = DateTimeOffset.Parse("2024-12-01T00:00:00+00:00"),
			PrivacyLevel = ScheduledEventPrivacyLevel.GuildOnly,
			Status = ScheduledEventStatus.Scheduled,
			EntityType = ScheduledEventEntityType.External,
			EntityMetadata = new ScheduledEventEntityMetadata { Location = "Park" },
		});

	[Fact]
	public async Task Modify_PatchesScheduledEventAsync()
	{
		Http.SendAsync<GuildScheduledEvent>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new GuildScheduledEvent { Id = _eventId, GuildId = _guildId, Name = "Renamed" });

		var ev = NewWrapper();
		await ev.Modify()
			.SetName("Renamed")
			.SetStatus(ScheduledEventStatus.Active)
			.ExecuteAsync();

		await Http.Received(1).SendAsync<GuildScheduledEvent>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/scheduled-events/{_eventId}"),
			HttpMethod.Patch,
			Arg.Is<object?>(b => BodyPropertyEquals(b, "Name", "Renamed") && BodyPropertyEquals(b, "Status", ScheduledEventStatus.Active)),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Delete_DeletesScheduledEventAsync()
	{
		var ev = NewWrapper();
		await ev.Delete().ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/scheduled-events/{_eventId}"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetUsers_GetsScheduledEventUsersAsync()
	{
		Http.SendAsync<GuildScheduledEventUser[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		var ev = NewWrapper();
		await ev.GetUsers(limit: 10, withMember: true).ExecuteAsync();

		await Http.Received(1).SendAsync<GuildScheduledEventUser[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/scheduled-events/{_eventId}/users?limit=10&with_member=true"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}
}

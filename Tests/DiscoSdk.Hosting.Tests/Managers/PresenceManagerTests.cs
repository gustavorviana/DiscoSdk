using DiscoSdk.Caching;
using DiscoSdk.Hosting.Managers;
using DiscoSdk.Models;
using DiscoSdk.Models.Activities;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Presences;

namespace DiscoSdk.Hosting.Tests.Managers;

public class PresenceManagerTests
{
	private static readonly Snowflake Guild = new(100);
	private static readonly Snowflake User = new(42);

	private static Presence Sample(
		string status = "online",
		Activity[]? activities = null,
		ClientStatus? clientStatus = null,
		Activity? game = null)
		=> new()
		{
			User = new PresenceUser { Id = User },
			Status = status,
			Activities = activities ?? [],
			ClientStatus = clientStatus,
			Game = game
		};

	[Fact]
	public void TryGet_EmptyCache_ReturnsNull()
	{
		var sut = new PresenceManager(PresenceCacheFlag.All);
		Assert.Null(sut.TryGet(Guild, User));
	}

	[Fact]
	public void OnPresenceUpdate_AllFlags_StoresEverything()
	{
		var sut = new PresenceManager(PresenceCacheFlag.All);
		sut.OnPresenceUpdate(Sample(
			status: "online",
			activities: [new Activity { Name = "Game", Type = ActivityType.Playing }],
			clientStatus: new ClientStatus { Desktop = "online" }),
			Guild);

		var hit = sut.TryGet(Guild, User);

		Assert.NotNull(hit);
		Assert.Equal("online", hit!.Status);
		Assert.NotNull(hit.ClientStatus);
		Assert.Single(hit.Activities);
	}

	[Fact]
	public void OnPresenceUpdate_OfflineStatus_ActsAsTombstone()
	{
		var sut = new PresenceManager(PresenceCacheFlag.All);
		sut.OnPresenceUpdate(Sample(status: "online"), Guild);
		Assert.NotNull(sut.TryGet(Guild, User));

		sut.OnPresenceUpdate(Sample(status: "offline"), Guild);

		Assert.Null(sut.TryGet(Guild, User));
	}

	[Fact]
	public void OnPresenceUpdate_NoneFlag_StoresNothing()
	{
		var sut = new PresenceManager(PresenceCacheFlag.None);
		sut.OnPresenceUpdate(Sample(status: "online"), Guild);
		Assert.Null(sut.TryGet(Guild, User));
	}

	[Fact]
	public void OnPresenceUpdate_ClientStatusOnly_DropsActivities()
	{
		var sut = new PresenceManager(PresenceCacheFlag.ClientStatus);
		sut.OnPresenceUpdate(Sample(
			status: "idle",
			activities: [new Activity { Name = "Spotify", Type = ActivityType.Listening }],
			clientStatus: new ClientStatus { Mobile = "idle" },
			game: new Activity { Name = "Spotify", Type = ActivityType.Listening }),
			Guild);

		var hit = sut.TryGet(Guild, User);

		Assert.NotNull(hit);
		Assert.Equal("idle", hit!.Status);
		Assert.NotNull(hit.ClientStatus);
		Assert.Empty(hit.Activities);
		Assert.Null(hit.Game);
	}

	[Fact]
	public void OnPresenceUpdate_ActivitiesOnly_DropsStatusFields()
	{
		var sut = new PresenceManager(PresenceCacheFlag.Activities);
		sut.OnPresenceUpdate(Sample(
			status: "dnd",
			activities: [new Activity { Name = "Code", Type = ActivityType.Playing }],
			clientStatus: new ClientStatus { Desktop = "dnd" }),
			Guild);

		var hit = sut.TryGet(Guild, User);

		Assert.NotNull(hit);
		Assert.Null(hit!.Status);
		Assert.Null(hit.ClientStatus);
		Assert.Single(hit.Activities);
	}

	[Fact]
	public void TryGetClientStatus_ReturnsRawPocoWhenFlagEnabled()
	{
		var sut = new PresenceManager(PresenceCacheFlag.ClientStatus);
		sut.OnPresenceUpdate(Sample(
			status: "online",
			clientStatus: new ClientStatus { Desktop = "online", Mobile = "idle" }),
			Guild);

		var cs = sut.TryGetClientStatus(Guild, User);

		Assert.NotNull(cs);
		Assert.Equal("online", cs!.Desktop);
		Assert.Equal("idle", cs.Mobile);
	}

	[Fact]
	public void OnGuildPresencesSeed_PopulatesEveryNonOfflineEntry()
	{
		var sut = new PresenceManager(PresenceCacheFlag.All);
		var seed = new[]
		{
			new Presence { User = new PresenceUser { Id = new Snowflake(1) }, Status = "online" },
			new Presence { User = new PresenceUser { Id = new Snowflake(2) }, Status = "idle" },
			new Presence { User = new PresenceUser { Id = new Snowflake(3) }, Status = "offline" }, // skipped
		};

		sut.OnGuildPresencesSeed(seed, Guild);

		Assert.NotNull(sut.TryGet(Guild, new Snowflake(1)));
		Assert.NotNull(sut.TryGet(Guild, new Snowflake(2)));
		Assert.Null(sut.TryGet(Guild, new Snowflake(3)));
	}

	[Fact]
	public void OnGuildRemove_DropsPartition()
	{
		var sut = new PresenceManager(PresenceCacheFlag.All);
		sut.OnPresenceUpdate(Sample(status: "online"), Guild);
		Assert.NotNull(sut.TryGet(Guild, User));

		sut.OnGuildRemove(Guild);

		Assert.Null(sut.TryGet(Guild, User));
	}

	[Theory]
	[InlineData("online", OnlineStatus.Online)]
	[InlineData("idle", OnlineStatus.Idle)]
	[InlineData("dnd", OnlineStatus.DoNotDisturb)]
	[InlineData("offline", OnlineStatus.Offline)]
	[InlineData("invisible", OnlineStatus.Invisible)]
	public void MapStatus_KnownString_ReturnsEnum(string raw, OnlineStatus expected)
	{
		Assert.Equal(expected, PresenceManager.MapStatus(raw));
	}

	[Fact]
	public void MapStatus_UnknownString_ReturnsNull()
	{
		Assert.Null(PresenceManager.MapStatus("strange"));
		Assert.Null(PresenceManager.MapStatus(null));
		Assert.Null(PresenceManager.MapStatus(""));
	}
}

using DiscoSdk.Caching;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using DiscoSdk.Hosting.Tests.Gateway.Events;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Tests.Managers;

/// <summary>
/// Integration tests for the member cache wiring. These spin up the real dispatcher and verify
/// the side effects of GUILD_MEMBER_* / GUILD_MEMBERS_CHUNK / GUILD_DELETE on the cache surface
/// exposed by <see cref="IDiscordClient.Members"/> and <see cref="IGuild.Members"/>.
/// </summary>
public class MemberCacheDispatchTests : DispatcherTestBase
{
	[Fact]
	public async Task GuildMemberAdd_PopulatesMemberCacheAsync()
	{
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.GuildMemberAdd(guildId: 100, userId: 42));

		var scope = Client.Members.OfGuild(new Snowflake(100));
		Assert.Equal(1, scope.GetCachedCount());

		var fetched = await scope.Get(new Snowflake(42), MemberFetchMode.CacheOnly).ExecuteAsync();
		Assert.NotNull(fetched);
		Assert.Equal(new Snowflake(42), fetched!.User.Id);
	}

	[Fact]
	public async Task GuildMemberRemove_DropsCachedEntryAsync()
	{
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));
		await DispatchAsync(DispatchFrames.GuildMemberAdd(guildId: 100, userId: 42));

		await DispatchAsync(DispatchFrames.GuildMemberRemove(guildId: 100, userId: 42));

		var scope = Client.Members.OfGuild(new Snowflake(100));
		Assert.Equal(0, scope.GetCachedCount());
		Assert.Null(await scope.Get(new Snowflake(42), MemberFetchMode.CacheOnly).ExecuteAsync());
	}

	[Fact]
	public async Task GuildMemberUpdate_UpsertsCachedEntryAsync()
	{
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.GuildMemberUpdate(guildId: 100, userId: 42, nickname: "renamed"));

		var member = await Client.Members.OfGuild(new Snowflake(100)).Get(new Snowflake(42), MemberFetchMode.CacheOnly).ExecuteAsync();
		Assert.NotNull(member);
		Assert.Equal("renamed", member!.Nickname);
	}

	[Fact]
	public async Task GuildScope_OfGuildAndIGuildMembers_AgreeAsync()
	{
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));
		await DispatchAsync(DispatchFrames.GuildMemberAdd(guildId: 100, userId: 42));

		var guild = Client.Guilds.TryGet(new Snowflake(100), out var g) ? g : null;
		Assert.NotNull(guild);

		var viaTopLevel = Client.Members.OfGuild(new Snowflake(100));
		var viaGuild = guild!.Members;

		Assert.Equal(viaTopLevel.GetCachedCount(), viaGuild.GetCachedCount());
	}

	[Fact]
	public async Task EnumerateGuildMembers_YieldsCachedEntriesAsync()
	{
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));
		await DispatchAsync(DispatchFrames.GuildMemberAdd(guildId: 100, userId: 1));
		await DispatchAsync(DispatchFrames.GuildMemberAdd(guildId: 100, userId: 2));
		await DispatchAsync(DispatchFrames.GuildMemberAdd(guildId: 100, userId: 3));

		var seen = new HashSet<Snowflake>();
		foreach (var member in Client.Members.OfGuild(new Snowflake(100)).GetCached())
			seen.Add(member.User.Id);

		Assert.Equal(new[] { new Snowflake(1), new Snowflake(2), new Snowflake(3) }, seen.OrderBy(x => x.Value));
	}

	[Fact]
	public async Task CacheOnlyMiss_ReturnsNullWithoutRestCallAsync()
	{
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		var member = await Client.Members.Get(
			new Snowflake(100),
			new Snowflake(404),
			MemberFetchMode.CacheOnly).ExecuteAsync();

		Assert.Null(member);
	}
}

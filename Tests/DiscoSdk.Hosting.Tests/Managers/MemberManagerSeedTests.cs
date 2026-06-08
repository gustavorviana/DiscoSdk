using DiscoSdk.Caching;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using DiscoSdk.Hosting.Tests.Gateway.Events;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Tests.Managers;

/// <summary>
/// Integration coverage for the GUILD_CREATE member seed path. Verifies the SDK ingests the
/// <c>members[]</c> array Discord ships in <c>GUILD_CREATE</c> instead of dropping it on the
/// floor, and that the array is cleared off the cached <c>Guild</c> POCO so the member manager
/// stays the single source of truth.
/// </summary>
public class MemberManagerSeedTests : DispatcherTestBase
{
	[Fact]
	public async Task GuildCreate_SeedsMembersIntoCacheAsync()
	{
		await DispatchAsync(DispatchFrames.GuildCreateWithMembers(
			id: 200,
			(1UL, "alice"),
			(2UL, "bob"),
			(3UL, "carol")));

		var scope = Client.Members.OfGuild(new Snowflake(200));
		Assert.Equal(3, scope.GetCachedCount());

		Assert.NotNull(await scope.Get(new Snowflake(1), MemberFetchMode.CacheOnly).ExecuteAsync());
		Assert.NotNull(await scope.Get(new Snowflake(2), MemberFetchMode.CacheOnly).ExecuteAsync());
		Assert.NotNull(await scope.Get(new Snowflake(3), MemberFetchMode.CacheOnly).ExecuteAsync());
	}

	[Fact]
	public async Task GuildCreate_LeavesNoMembersOnTheCachedGuildPocoAsync()
	{
		await DispatchAsync(DispatchFrames.GuildCreateWithMembers(id: 200, (1UL, "alice")));

		Assert.True(Client.Guilds.TryGet(new Snowflake(200), out var guild));
		Assert.NotNull(guild);

		// IGuild has no Members[] surface — the cached Guild POCO behind the wrapper should also
		// be empty. Touch it via reflection to pin the "single source of truth" invariant.
		var wrapperType = guild!.GetType();
		var guildField = wrapperType.GetField("_guild",
			System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
		var pocoGuild = guildField!.GetValue(guild);

		var membersProp = pocoGuild!.GetType().GetProperty("Members");
		var pocoMembers = (DiscoSdk.Models.GuildMember[])membersProp!.GetValue(pocoGuild)!;

		Assert.Empty(pocoMembers);
	}
}

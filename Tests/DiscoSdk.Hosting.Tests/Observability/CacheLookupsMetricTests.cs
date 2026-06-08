using DiscoSdk.Caching;
using DiscoSdk.Hosting.Observability;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using DiscoSdk.Hosting.Tests.Gateway.Events;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Tests.Observability;

[Collection("Observability")]
public class CacheLookupsMetricTests : DispatcherTestBase
{
	[Fact]
	public async Task MemberCacheHit_PublishesHitRowAsync()
	{
		using var capture = new MeterListenerCapture("discosdk.cache.lookups");
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));
		await DispatchAsync(DispatchFrames.GuildMemberAdd(guildId: 100, userId: 42));

		var member = await Client.Members.OfGuild(new Snowflake(100))
			.GetAsync(new Snowflake(42), MemberFetchMode.CacheOnly);

		Assert.NotNull(member);
		var rows = capture.LongFor("discosdk.cache.lookups").ToList();
		Assert.Contains(rows, m =>
			Equals(m.Tag(DiagnosticTags.CacheEntity), DiagnosticTags.CacheEntityMember) &&
			Equals(m.Tag(DiagnosticTags.CacheResult), "hit"));
	}

	[Fact]
	public async Task MemberCacheMiss_PublishesMissRowAsync()
	{
		using var capture = new MeterListenerCapture("discosdk.cache.lookups");
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		var member = await Client.Members.OfGuild(new Snowflake(100))
			.GetAsync(new Snowflake(404), MemberFetchMode.CacheOnly);

		Assert.Null(member);
		var rows = capture.LongFor("discosdk.cache.lookups").ToList();
		Assert.Contains(rows, m =>
			Equals(m.Tag(DiagnosticTags.CacheEntity), DiagnosticTags.CacheEntityMember) &&
			Equals(m.Tag(DiagnosticTags.CacheResult), "miss"));
	}
}

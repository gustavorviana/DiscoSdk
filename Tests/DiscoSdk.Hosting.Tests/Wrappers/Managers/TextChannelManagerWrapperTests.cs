using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers.Managers;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using DiscoSdk.Rest.Actions;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Managers;

public class TextChannelManagerWrapperTests : WrapperTestBase
{
	[Fact]
	public async Task ExecuteAsync_PatchesChannelWithTextSettersAsync()
	{
		Http.SendAsync<Channel>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Channel());
		var mgr = new TextChannelManagerWrapper(new Snowflake(200), Client.ChannelClient);

		mgr.SetTopic("topic-x");
		mgr.SetDefaultAutoArchiveDuration(ThreadAutoArchiveDuration.OneWeek);
		mgr.SetNsfw(true);
		mgr.SetRateLimitPerUser(Slowmode.FromSeconds(15));
		await ((IRestAction)mgr).ExecuteAsync();

		await Http.Received(1).SendAsync<Channel>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200"),
			HttpMethod.Patch,
			Arg.Is<object?>(body =>
				BodyContains(body, "topic", "topic-x") &&
				BodyContains(body, "default_auto_archive_duration", (int)ThreadAutoArchiveDuration.OneWeek) &&
				BodyContains(body, "nsfw", true) &&
				BodyContains(body, "rate_limit_per_user", 15)),
			Arg.Any<CancellationToken>());
	}
}

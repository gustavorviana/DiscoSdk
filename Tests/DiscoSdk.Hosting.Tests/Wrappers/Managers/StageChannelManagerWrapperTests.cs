using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers.Managers;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Rest;
using DiscoSdk.Rest.Actions;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Managers;

public class StageChannelManagerWrapperTests : WrapperTestBase
{
	[Fact]
	public async Task ExecuteAsync_PatchesChannelWithStageSettersAsync()
	{
		Http.SendAsync<Channel>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Channel());
		var mgr = new StageChannelManagerWrapper(new Snowflake(200), Client.ChannelClient);

		mgr.SetBitrate(96000);
		mgr.SetRegion("us-east");
		await ((IRestAction)mgr).ExecuteAsync();

		await Http.Received(1).SendAsync<Channel>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200"),
			HttpMethod.Patch,
			Arg.Is<object?>(body =>
				BodyContains(body, "bitrate", 96000) &&
				BodyContains(body, "rtc_region", "us-east")),
			Arg.Any<CancellationToken>());
	}
}

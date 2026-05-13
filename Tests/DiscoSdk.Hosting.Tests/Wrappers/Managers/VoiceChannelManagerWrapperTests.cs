using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers.Managers;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Rest;
using DiscoSdk.Rest.Actions;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Managers;

public class VoiceChannelManagerWrapperTests : WrapperTestBase
{
	[Fact]
	public async Task ExecuteAsync_PatchesChannelWithVoiceSettersAsync()
	{
		Http.SendAsync<Channel>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Channel());
		var mgr = new VoiceChannelManagerWrapper(new Snowflake(200), Client.ChannelClient);

		mgr.SetBitrate(64000);
		mgr.SetUserLimit(10);
		mgr.SetRegion("brazil");
		await ((IRestAction)mgr).ExecuteAsync();

		await Http.Received(1).SendAsync<Channel>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200"),
			HttpMethod.Patch,
			Arg.Is<object?>(body =>
				BodyContains(body, "bitrate", 64000) &&
				BodyContains(body, "user_limit", 10) &&
				BodyContains(body, "rtc_region", "brazil")),
			Arg.Any<CancellationToken>());
	}
}

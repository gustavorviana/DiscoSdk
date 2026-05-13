using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers.Managers;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using DiscoSdk.Rest.Actions;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Managers;

public class ThreadChannelManagerWrapperTests : WrapperTestBase
{
	[Fact]
	public async Task ExecuteAsync_PatchesChannelWithThreadSettersAsync()
	{
		Http.SendAsync<Channel>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Channel());
		var mgr = new ThreadChannelManagerWrapper(new Snowflake(200), Client.ChannelClient);

		mgr.SetArchived(true);
		mgr.SetLocked(true);
		mgr.SetInvitable(false);
		mgr.SetAutoArchiveDuration(ThreadAutoArchiveDuration.OneDay);
		mgr.SetSlowmode(Slowmode.FromSeconds(5));
		await ((IRestAction)mgr).ExecuteAsync();

		await Http.Received(1).SendAsync<Channel>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200"),
			HttpMethod.Patch,
			Arg.Is<object?>(body =>
				BodyContains(body, "archived", true) &&
				BodyContains(body, "locked", true) &&
				BodyContains(body, "invitable", false) &&
				BodyContains(body, "auto_archive_duration", (int)ThreadAutoArchiveDuration.OneDay) &&
				BodyContains(body, "rate_limit_per_user", 5)),
			Arg.Any<CancellationToken>());
	}
}

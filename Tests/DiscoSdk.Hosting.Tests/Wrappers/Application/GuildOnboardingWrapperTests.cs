using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Application;

public class GuildOnboardingWrapperTests : WrapperTestBase
{
	[Fact]
	public async Task Modify_PutsOnboardingWithChangesAsync()
	{
		Http.SendAsync<GuildOnboarding>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new GuildOnboarding { GuildId = new Snowflake(100) });
		var wrapper = new GuildOnboardingWrapper(Client, new GuildOnboarding { GuildId = new Snowflake(100) });

		await wrapper.Modify()
			.SetEnabled(true)
			.SetMode(OnboardingMode.OnboardingAdvanced)
			.SetDefaultChannelIds(new Snowflake(1), new Snowflake(2))
			.ExecuteAsync();

		await Http.Received(1).SendAsync<GuildOnboarding>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/onboarding"),
			HttpMethod.Put,
			Arg.Is<object?>(b =>
				BodyContains(b, "enabled", true) &&
				BodyContains(b, "mode", (int)OnboardingMode.OnboardingAdvanced) &&
				BodyHasKey(b, "default_channel_ids")),
			Arg.Any<CancellationToken>());
	}
}

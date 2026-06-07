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

	[Fact]
	public async Task Modify_WithReason_RoutesThroughReasonedSendAsync()
	{
		Http.SendWithReasonAsync<GuildOnboarding>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(new GuildOnboarding { GuildId = new Snowflake(100) });
		var wrapper = new GuildOnboardingWrapper(Client, new GuildOnboarding { GuildId = new Snowflake(100) });

		await wrapper.Modify()
			.SetEnabled(true)
			.WithReason("Quarterly onboarding refresh")
			.ExecuteAsync();

		await Http.Received(1).SendWithReasonAsync<GuildOnboarding>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/onboarding"),
			HttpMethod.Put,
			Arg.Any<object?>(),
			"Quarterly onboarding refresh",
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Modify_NoChanges_ThrowsAsync()
	{
		var wrapper = new GuildOnboardingWrapper(Client, new GuildOnboarding { GuildId = new Snowflake(100) });
		await Assert.ThrowsAsync<InvalidOperationException>(() => wrapper.Modify().ExecuteAsync());
	}

	[Fact]
	public async Task Modify_AddPromptInlineAccumulatesAsync()
	{
		Http.SendAsync<GuildOnboarding>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new GuildOnboarding { GuildId = new Snowflake(100) });
		var wrapper = new GuildOnboardingWrapper(Client, new GuildOnboarding { GuildId = new Snowflake(100) });

		await wrapper.Modify()
			.AddPrompt(p => p
				.SetTitle("Tribe")
				.AddOption(o => o.SetTitle("Devs")))
			.AddPrompt(p => p
				.SetTitle("Region")
				.AddOption(o => o.SetTitle("US")))
			.ExecuteAsync();

		await Http.Received(1).SendAsync<GuildOnboarding>(
			Arg.Any<DiscordRoute>(), HttpMethod.Put,
			Arg.Is<object?>(b => BodyHasKey(b, "prompts")),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Modify_SetPromptsFromBuilders_FlattensThroughBuildAsync()
	{
		Http.SendAsync<GuildOnboarding>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new GuildOnboarding { GuildId = new Snowflake(100) });
		var wrapper = new GuildOnboardingWrapper(Client, new GuildOnboarding { GuildId = new Snowflake(100) });

		var prompt = new OnboardingPromptBuilder()
			.SetTitle("Pick a tribe")
			.SetType(OnboardingPromptType.MultipleChoice)
			.AddOption(new OnboardingPromptOptionBuilder()
				.SetTitle("Devs")
				.AddRole(new Snowflake(900))
				.SetUnicodeEmoji("🛠️"));

		await wrapper.Modify().SetPrompts(prompt).ExecuteAsync();

		await Http.Received(1).SendAsync<GuildOnboarding>(
			Arg.Any<DiscordRoute>(), HttpMethod.Put,
			Arg.Is<object?>(b => BodyHasKey(b, "prompts")),
			Arg.Any<CancellationToken>());
	}
}

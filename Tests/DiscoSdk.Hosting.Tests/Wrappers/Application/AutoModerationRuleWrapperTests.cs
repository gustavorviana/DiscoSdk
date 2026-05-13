using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.AutoModeration;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Application;

public class AutoModerationRuleWrapperTests : WrapperTestBase
{
	private static AutoModerationRule Model() => new()
	{
		Id = new Snowflake(42),
		GuildId = new Snowflake(100),
		Actions = [],
		ExemptRoles = [],
		ExemptChannels = [],
	};

	[Fact]
	public async Task Modify_PatchesRuleWithChangesAsync()
	{
		Http.SendAsync<AutoModerationRule>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(Model());
		var wrapper = new AutoModerationRuleWrapper(Client, Model());

		await wrapper.Modify()
			.SetName("new")
			.SetEnabled(false)
			.SetEventType(AutoModerationEventType.MessageSend)
			.ExecuteAsync();

		await Http.Received(1).SendAsync<AutoModerationRule>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/auto-moderation/rules/42"),
			HttpMethod.Patch,
			Arg.Is<object?>(b =>
				BodyContains(b, "name", "new") &&
				BodyContains(b, "enabled", false) &&
				BodyContains(b, "event_type", (int)AutoModerationEventType.MessageSend)),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Delete_DeletesRuleAsync()
	{
		var wrapper = new AutoModerationRuleWrapper(Client, Model());

		await wrapper.Delete().ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/auto-moderation/rules/42"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}
}

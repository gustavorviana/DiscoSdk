using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Events;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using DiscoSdk.Models;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Gateway.Events;

public class AutoModerationDispatchTests : DispatcherTestBase
{
	[Fact]
	public async Task RuleCreate_InvokesIAutoModerationRuleCreateHandlerAsync()
	{
		var handler = Substitute.For<IAutoModerationRuleCreateHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.AutoModRuleCreate(id: 600, guildId: 100));

		await handler.Received(1).HandleAsync(
			Arg.Is<IAutoModerationRuleContext>(ctx => ctx.Rule.Id == new Snowflake(600)),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task RuleUpdate_InvokesIAutoModerationRuleUpdateHandlerAsync()
	{
		var handler = Substitute.For<IAutoModerationRuleUpdateHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.AutoModRuleUpdate(id: 600, guildId: 100, name: "renamed"));

		await handler.Received(1).HandleAsync(
			Arg.Is<IAutoModerationRuleContext>(ctx => ctx.Rule.Name == "renamed"),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task RuleDelete_InvokesIAutoModerationRuleDeleteHandlerAsync()
	{
		var handler = Substitute.For<IAutoModerationRuleDeleteHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.AutoModRuleDelete(id: 600, guildId: 100));

		await handler.Received(1).HandleAsync(
			Arg.Is<IAutoModerationRuleContext>(ctx => ctx.Rule.Id == new Snowflake(600)),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task ActionExecution_InvokesIAutoModerationActionExecutionHandlerAsync()
	{
		var handler = Substitute.For<IAutoModerationActionExecutionHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.AutoModActionExecution(guildId: 100, ruleId: 600, userId: 42, matchedContent: "spam"));

		await handler.Received(1).HandleAsync(
			Arg.Is<IAutoModerationActionExecutionContext>(ctx =>
				ctx.RuleId == new Snowflake(600) &&
				ctx.UserId == new Snowflake(42) &&
				ctx.MatchedContent == "spam"),
			Arg.Any<IServiceProvider>());
	}
}

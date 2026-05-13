using DiscoSdk.Contexts.Messages;
using DiscoSdk.Events;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using DiscoSdk.Models;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Gateway.Events;

public class ReactionDispatchTests : DispatcherTestBase
{
	[Fact]
	public async Task MessageReactionAdd_InvokesIMessageReactionAddHandlerAsync()
	{
		var handler = Substitute.For<IMessageReactionAddHandler>();
		AddHandler(handler);

		// Seed guild — GuildMemberWrapper throws if the guild isn't cached, and the swallowed
		// exception inside the dispatcher would otherwise prevent the handler from running.
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));
		await DispatchAsync(DispatchFrames.MessageReactionAdd(userId: 42, channelId: 200, guildId: 100, messageId: 300, emojiName: "smile"));

		await handler.Received(1).HandleAsync(
			Arg.Is<IMessageAddReactionContext>(ctx =>
				ctx.MessageId == new Snowflake(300) &&
				ctx.Emoji.Name == "smile"),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task MessageReactionRemove_InvokesIMessageReactionRemoveHandlerAsync()
	{
		var handler = Substitute.For<IMessageReactionRemoveHandler>();
		AddHandler(handler);

		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));
		await DispatchAsync(DispatchFrames.MessageReactionRemove(userId: 42, channelId: 200, guildId: 100, messageId: 300, emojiName: "smile"));

		await handler.Received(1).HandleAsync(
			Arg.Is<IMessageDeleteReactionContext>(ctx =>
				ctx.MessageId == new Snowflake(300) &&
				ctx.Emoji.Name == "smile"),
			Arg.Any<IServiceProvider>());
	}
}

using DiscoSdk.Contexts.Messages;
using DiscoSdk.Events;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using DiscoSdk.Models;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Gateway.Events;

/// <summary>
/// Verifies the MESSAGE_DELETE_BULK and MESSAGE_REACTION_REMOVE_ALL/EMOJI dispatch routes.
/// </summary>
public class MessageVariantsDispatchTests : DispatcherTestBase
{
	[Fact]
	public async Task MessageDeleteBulk_InvokesIMessageDeleteBulkHandlerAsync()
	{
		var handler = Substitute.For<IMessageDeleteBulkHandler>();
		AddHandler(handler);

		await DispatchAsync(DispatchFrames.MessageDeleteBulk(channelId: 200, guildId: 100, ids: [301UL, 302UL, 303UL]));

		await handler.Received(1).HandleAsync(
			Arg.Is<IMessageDeleteBulkContext>(ctx =>
				ctx.Ids.Length == 3 &&
				ctx.Ids.Contains(new Snowflake(301)) &&
				ctx.Ids.Contains(new Snowflake(302)) &&
				ctx.Ids.Contains(new Snowflake(303))),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task MessageReactionRemoveAll_InvokesIMessageReactionRemoveAllHandlerAsync()
	{
		var handler = Substitute.For<IMessageReactionRemoveAllHandler>();
		AddHandler(handler);

		await DispatchAsync(DispatchFrames.MessageReactionRemoveAll(channelId: 200, messageId: 300));

		await handler.Received(1).HandleAsync(
			Arg.Is<IMessageReactionRemoveAllContext>(ctx => ctx.MessageId == new Snowflake(300)),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task MessageReactionRemoveEmoji_InvokesIMessageReactionRemoveEmojiHandlerAsync()
	{
		var handler = Substitute.For<IMessageReactionRemoveEmojiHandler>();
		AddHandler(handler);

		await DispatchAsync(DispatchFrames.MessageReactionRemoveEmoji(channelId: 200, messageId: 300, emojiName: "smile"));

		await handler.Received(1).HandleAsync(
			Arg.Is<IMessageReactionRemoveEmojiContext>(ctx =>
				ctx.MessageId == new Snowflake(300) &&
				ctx.Emoji.Name == "smile"),
			Arg.Any<IServiceProvider>());
	}
}

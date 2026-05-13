using DiscoSdk.Contexts.Messages;
using DiscoSdk.Events;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using DiscoSdk.Models;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Gateway.Events;

public class PollVoteDispatchTests : DispatcherTestBase
{
	[Fact]
	public async Task PollVoteAdd_InvokesIMessagePollVoteAddHandlerAsync()
	{
		var handler = Substitute.For<IMessagePollVoteAddHandler>();
		AddHandler(handler);

		await DispatchAsync(DispatchFrames.MessagePollVoteAdd(userId: 42, messageId: 300, answerId: 2));

		await handler.Received(1).HandleAsync(
			Arg.Is<IMessagePollVoteContext>(ctx =>
				ctx.UserId == new Snowflake(42) &&
				ctx.MessageId == new Snowflake(300) &&
				ctx.AnswerId == 2),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task PollVoteRemove_InvokesIMessagePollVoteRemoveHandlerAsync()
	{
		var handler = Substitute.For<IMessagePollVoteRemoveHandler>();
		AddHandler(handler);

		await DispatchAsync(DispatchFrames.MessagePollVoteRemove(userId: 42, messageId: 300, answerId: 2));

		await handler.Received(1).HandleAsync(
			Arg.Is<IMessagePollVoteContext>(ctx =>
				ctx.UserId == new Snowflake(42) &&
				ctx.MessageId == new Snowflake(300) &&
				ctx.AnswerId == 2),
			Arg.Any<IServiceProvider>());
	}
}

using DiscoSdk.Contexts.Messages;
using DiscoSdk.Events;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using DiscoSdk.Models;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Gateway.Events;

public class MessageDispatchTests : DispatcherTestBase
{
	[Fact]
	public async Task MessageDelete_InvokesIMessageDeleteHandlerAsync()
	{
		var handler = Substitute.For<IMessageDeleteHandler>();
		AddHandler(handler);

		await DispatchAsync(DispatchFrames.MessageDelete(id: 300, channelId: 200, guildId: 100));

		await handler.Received(1).HandleAsync(
			Arg.Is<IMessageDeleteContext>(ctx => ctx.Id == new Snowflake(300)),
			Arg.Any<IServiceProvider>());
	}
}

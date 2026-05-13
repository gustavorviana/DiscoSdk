using DiscoSdk.Contexts.Channels;
using DiscoSdk.Events;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using DiscoSdk.Models;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Gateway.Events;

public class TypingDispatchTests : DispatcherTestBase
{
	[Fact]
	public async Task TypingStart_InvokesITypingStartHandlerAsync()
	{
		var handler = Substitute.For<ITypingStartHandler>();
		AddHandler(handler);

		// Seed guild so the dispatcher's GuildMemberWrapper construction succeeds.
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));
		await DispatchAsync(DispatchFrames.TypingStart(userId: 42, channelId: 200, guildId: 100, timestamp: 1700000000));

		await handler.Received(1).HandleAsync(
			Arg.Any<ITypingContext>(),
			Arg.Any<IServiceProvider>());
	}
}

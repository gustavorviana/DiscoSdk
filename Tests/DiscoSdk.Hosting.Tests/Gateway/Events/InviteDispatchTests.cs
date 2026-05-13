using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Events;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Gateway.Events;

public class InviteDispatchTests : DispatcherTestBase
{
	[Fact]
	public async Task InviteCreate_InvokesIInviteCreateHandlerAsync()
	{
		var handler = Substitute.For<IInviteCreateHandler>();
		AddHandler(handler);

		await DispatchAsync(DispatchFrames.InviteCreate(code: "abc123", channelId: 200, guildId: 100));

		await handler.Received(1).HandleAsync(
			Arg.Is<IInviteCreateContext>(ctx =>
				ctx.Code == "abc123" &&
				ctx.MaxAge == 3600 &&
				ctx.MaxUses == 5),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task InviteDelete_InvokesIInviteDeleteHandlerAsync()
	{
		var handler = Substitute.For<IInviteDeleteHandler>();
		AddHandler(handler);

		await DispatchAsync(DispatchFrames.InviteDelete(code: "abc123", channelId: 200));

		await handler.Received(1).HandleAsync(
			Arg.Is<IInviteDeleteContext>(ctx => ctx.Code == "abc123"),
			Arg.Any<IServiceProvider>());
	}
}

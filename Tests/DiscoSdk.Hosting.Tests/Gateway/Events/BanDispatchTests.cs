using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Events;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using DiscoSdk.Models;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Gateway.Events;

public class BanDispatchTests : DispatcherTestBase
{
	[Fact]
	public async Task GuildBanAdd_InvokesIGuildBanAddHandlerAsync()
	{
		var handler = Substitute.For<IGuildBanAddHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.GuildBanAdd(guildId: 100, userId: 42));

		await handler.Received(1).HandleAsync(
			Arg.Is<IGuildBanContext>(ctx => ctx.User.Id == new Snowflake(42)),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task GuildBanRemove_InvokesIGuildBanRemoveHandlerAsync()
	{
		var handler = Substitute.For<IGuildBanRemoveHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.GuildBanRemove(guildId: 100, userId: 42));

		await handler.Received(1).HandleAsync(
			Arg.Is<IGuildBanContext>(ctx => ctx.User.Id == new Snowflake(42)),
			Arg.Any<IServiceProvider>());
	}
}

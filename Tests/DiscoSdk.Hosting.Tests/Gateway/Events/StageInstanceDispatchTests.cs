using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Events;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using DiscoSdk.Models;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Gateway.Events;

public class StageInstanceDispatchTests : DispatcherTestBase
{
	[Fact]
	public async Task StageInstanceCreate_InvokesIStageInstanceCreateHandlerAsync()
	{
		var handler = Substitute.For<IStageInstanceCreateHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.StageInstanceCreate(id: 500, guildId: 100));

		await handler.Received(1).HandleAsync(
			Arg.Is<IStageInstanceContext>(ctx => ctx.Instance.Id == new Snowflake(500)),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task StageInstanceUpdate_InvokesIStageInstanceUpdateHandlerAsync()
	{
		var handler = Substitute.For<IStageInstanceUpdateHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.StageInstanceUpdate(id: 500, guildId: 100, topic: "Renamed"));

		await handler.Received(1).HandleAsync(
			Arg.Is<IStageInstanceContext>(ctx => ctx.Instance.Topic == "Renamed"),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task StageInstanceDelete_InvokesIStageInstanceDeleteHandlerAsync()
	{
		var handler = Substitute.For<IStageInstanceDeleteHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.StageInstanceDelete(id: 500, guildId: 100));

		await handler.Received(1).HandleAsync(
			Arg.Is<IStageInstanceContext>(ctx => ctx.Instance.Id == new Snowflake(500)),
			Arg.Any<IServiceProvider>());
	}
}

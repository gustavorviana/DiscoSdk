using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Events;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using DiscoSdk.Models;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Gateway.Events;

public class IntegrationDispatchTests : DispatcherTestBase
{
	[Fact]
	public async Task IntegrationCreate_InvokesIIntegrationCreateHandlerAsync()
	{
		var handler = Substitute.For<IIntegrationCreateHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.IntegrationCreate(id: 700, guildId: 100));

		await handler.Received(1).HandleAsync(
			Arg.Is<IIntegrationContext>(ctx => ctx.Integration.Id == new Snowflake(700)),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task IntegrationUpdate_InvokesIIntegrationUpdateHandlerAsync()
	{
		var handler = Substitute.For<IIntegrationUpdateHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.IntegrationUpdate(id: 700, guildId: 100, name: "renamed"));

		await handler.Received(1).HandleAsync(
			Arg.Is<IIntegrationContext>(ctx => ctx.Integration.Name == "renamed"),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task IntegrationDelete_InvokesIIntegrationDeleteHandlerAsync()
	{
		var handler = Substitute.For<IIntegrationDeleteHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.IntegrationDelete(id: 700, guildId: 100, applicationId: 901));

		await handler.Received(1).HandleAsync(
			Arg.Is<IIntegrationDeleteContext>(ctx =>
				ctx.IntegrationId == new Snowflake(700) &&
				ctx.ApplicationId == new Snowflake(901)),
			Arg.Any<IServiceProvider>());
	}
}

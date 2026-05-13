using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Events;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using DiscoSdk.Models;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Gateway.Events;

public class EntitlementDispatchTests : DispatcherTestBase
{
	[Fact]
	public async Task EntitlementCreate_InvokesIEntitlementCreateHandlerAsync()
	{
		var handler = Substitute.For<IEntitlementCreateHandler>();
		AddHandler(handler);

		await DispatchAsync(DispatchFrames.EntitlementCreate(id: 900));

		await handler.Received(1).HandleAsync(
			Arg.Is<IEntitlementContext>(ctx => ctx.Entitlement.Id == new Snowflake(900)),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task EntitlementUpdate_InvokesIEntitlementUpdateHandlerAsync()
	{
		var handler = Substitute.For<IEntitlementUpdateHandler>();
		AddHandler(handler);

		await DispatchAsync(DispatchFrames.EntitlementUpdate(id: 900));

		await handler.Received(1).HandleAsync(
			Arg.Is<IEntitlementContext>(ctx => ctx.Entitlement.Id == new Snowflake(900)),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task EntitlementDelete_InvokesIEntitlementDeleteHandlerAsync()
	{
		var handler = Substitute.For<IEntitlementDeleteHandler>();
		AddHandler(handler);

		await DispatchAsync(DispatchFrames.EntitlementDelete(id: 900));

		await handler.Received(1).HandleAsync(
			Arg.Is<IEntitlementContext>(ctx => ctx.Entitlement.Id == new Snowflake(900)),
			Arg.Any<IServiceProvider>());
	}
}

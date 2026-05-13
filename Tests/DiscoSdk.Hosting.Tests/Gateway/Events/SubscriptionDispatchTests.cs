using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Events;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using DiscoSdk.Models;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Gateway.Events;

public class SubscriptionDispatchTests : DispatcherTestBase
{
	[Fact]
	public async Task SubscriptionCreate_InvokesISubscriptionCreateHandlerAsync()
	{
		var handler = Substitute.For<ISubscriptionCreateHandler>();
		AddHandler(handler);

		await DispatchAsync(DispatchFrames.SubscriptionCreate(id: 1300));

		await handler.Received(1).HandleAsync(
			Arg.Is<ISubscriptionContext>(ctx => ctx.Subscription.Id == new Snowflake(1300)),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task SubscriptionUpdate_InvokesISubscriptionUpdateHandlerAsync()
	{
		var handler = Substitute.For<ISubscriptionUpdateHandler>();
		AddHandler(handler);

		await DispatchAsync(DispatchFrames.SubscriptionUpdate(id: 1300));

		await handler.Received(1).HandleAsync(
			Arg.Is<ISubscriptionContext>(ctx => ctx.Subscription.Id == new Snowflake(1300)),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task SubscriptionDelete_InvokesISubscriptionDeleteHandlerAsync()
	{
		var handler = Substitute.For<ISubscriptionDeleteHandler>();
		AddHandler(handler);

		await DispatchAsync(DispatchFrames.SubscriptionDelete(id: 1300));

		await handler.Received(1).HandleAsync(
			Arg.Is<ISubscriptionContext>(ctx => ctx.Subscription.Id == new Snowflake(1300)),
			Arg.Any<IServiceProvider>());
	}
}

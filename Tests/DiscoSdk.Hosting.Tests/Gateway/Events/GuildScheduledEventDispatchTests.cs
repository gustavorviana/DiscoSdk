using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Events;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using DiscoSdk.Models;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Gateway.Events;

public class GuildScheduledEventDispatchTests : DispatcherTestBase
{
	[Fact]
	public async Task ScheduledEventCreate_InvokesIGuildScheduledEventCreateHandlerAsync()
	{
		var handler = Substitute.For<IGuildScheduledEventCreateHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.ScheduledEventCreate(id: 800, guildId: 100));

		await handler.Received(1).HandleAsync(
			Arg.Is<IGuildScheduledEventContext>(ctx => ctx.ScheduledEvent.Id == new Snowflake(800)),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task ScheduledEventUpdate_InvokesIGuildScheduledEventUpdateHandlerAsync()
	{
		var handler = Substitute.For<IGuildScheduledEventUpdateHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.ScheduledEventUpdate(id: 800, guildId: 100, name: "renamed"));

		await handler.Received(1).HandleAsync(
			Arg.Is<IGuildScheduledEventContext>(ctx => ctx.ScheduledEvent.Name == "renamed"),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task ScheduledEventDelete_InvokesIGuildScheduledEventDeleteHandlerAsync()
	{
		var handler = Substitute.For<IGuildScheduledEventDeleteHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.ScheduledEventDelete(id: 800, guildId: 100));

		await handler.Received(1).HandleAsync(
			Arg.Is<IGuildScheduledEventContext>(ctx => ctx.ScheduledEvent.Id == new Snowflake(800)),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task ScheduledEventUserAdd_InvokesIGuildScheduledEventUserAddHandlerAsync()
	{
		var handler = Substitute.For<IGuildScheduledEventUserAddHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.ScheduledEventUserAdd(eventId: 800, userId: 42, guildId: 100));

		await handler.Received(1).HandleAsync(
			Arg.Is<IGuildScheduledEventUserContext>(ctx =>
				ctx.ScheduledEventId == new Snowflake(800) &&
				ctx.UserId == new Snowflake(42)),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task ScheduledEventUserRemove_InvokesIGuildScheduledEventUserRemoveHandlerAsync()
	{
		var handler = Substitute.For<IGuildScheduledEventUserRemoveHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.ScheduledEventUserRemove(eventId: 800, userId: 42, guildId: 100));

		await handler.Received(1).HandleAsync(
			Arg.Is<IGuildScheduledEventUserContext>(ctx =>
				ctx.ScheduledEventId == new Snowflake(800) &&
				ctx.UserId == new Snowflake(42)),
			Arg.Any<IServiceProvider>());
	}
}

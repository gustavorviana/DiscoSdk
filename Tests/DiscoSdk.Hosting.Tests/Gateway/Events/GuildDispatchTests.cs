using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Events;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using DiscoSdk.Models;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Gateway.Events;

/// <summary>
/// Verifies GUILD_CREATE / GUILD_UPDATE / GUILD_DELETE dispatch frames reach the corresponding
/// handlers with the right guild data.
/// </summary>
public class GuildDispatchTests : DispatcherTestBase
{
	[Fact]
	public async Task GuildCreate_InvokesIGuildCreateHandlerAsync()
	{
		var handler = Substitute.For<IGuildCreateHandler>();
		AddHandler(handler);

		await DispatchAsync(DispatchFrames.GuildCreate(id: 100, name: "Test"));

		await handler.Received(1).HandleAsync(
			Arg.Is<IGuildContext>(ctx => ctx.Guild.Id == new Snowflake(100) && ctx.Guild.Name == "Test"),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task GuildUpdate_InvokesIGuildUpdateHandlerAsync()
	{
		var handler = Substitute.For<IGuildUpdateHandler>();
		AddHandler(handler);

		// Seed the guild first so the manager has it cached.
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.GuildUpdate(id: 100, name: "Renamed"));

		await handler.Received(1).HandleAsync(
			Arg.Is<IGuildContext>(ctx => ctx.Guild.Id == new Snowflake(100) && ctx.Guild.Name == "Renamed"),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task GuildDelete_InvokesIGuildDeleteHandlerAsync()
	{
		var handler = Substitute.For<IGuildDeleteHandler>();
		AddHandler(handler);

		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));
		await DispatchAsync(DispatchFrames.GuildDelete(id: 100));

		await handler.Received(1).HandleAsync(
			Arg.Is<IGuildDeleteContext>(ctx => ctx.Id == new Snowflake(100)),
			Arg.Any<IServiceProvider>());
	}
}

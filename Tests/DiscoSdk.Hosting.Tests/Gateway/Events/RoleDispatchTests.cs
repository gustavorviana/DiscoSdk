using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Events;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using DiscoSdk.Models;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Gateway.Events;

public class RoleDispatchTests : DispatcherTestBase
{
	[Fact]
	public async Task GuildRoleCreate_InvokesIGuildRoleCreateHandlerAsync()
	{
		var handler = Substitute.For<IGuildRoleCreateHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.GuildRoleCreate(guildId: 100, roleId: 500, name: "Mods"));

		await handler.Received(1).HandleAsync(
			Arg.Is<IGuildRoleContext>(ctx => ctx.Role.Id == new Snowflake(500) && ctx.Role.Name == "Mods"),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task GuildRoleUpdate_InvokesIGuildRoleUpdateHandlerAsync()
	{
		var handler = Substitute.For<IGuildRoleUpdateHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.GuildRoleUpdate(guildId: 100, roleId: 500, name: "Renamed"));

		await handler.Received(1).HandleAsync(
			Arg.Is<IGuildRoleContext>(ctx => ctx.Role.Id == new Snowflake(500) && ctx.Role.Name == "Renamed"),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task GuildRoleDelete_InvokesIGuildRoleDeleteHandlerAsync()
	{
		var handler = Substitute.For<IGuildRoleDeleteHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.GuildRoleDelete(guildId: 100, roleId: 500));

		await handler.Received(1).HandleAsync(
			Arg.Is<IGuildRoleDeleteContext>(ctx => ctx.RoleId == new Snowflake(500)),
			Arg.Any<IServiceProvider>());
	}
}

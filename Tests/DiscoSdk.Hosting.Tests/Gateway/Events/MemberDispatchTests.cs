using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Events;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using DiscoSdk.Models;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Gateway.Events;

/// <summary>
/// Verifies the GUILD_MEMBER_* dispatch path. Each test seeds a GUILD_CREATE first because the
/// dispatcher exits early when the guild isn't cached (member events fan out from a known guild).
/// </summary>
public class MemberDispatchTests : DispatcherTestBase
{
	[Fact]
	public async Task GuildMemberAdd_InvokesIGuildMemberAddHandlerAsync()
	{
		var handler = Substitute.For<IGuildMemberAddHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.GuildMemberAdd(guildId: 100, userId: 42));

		await handler.Received(1).HandleAsync(
			Arg.Is<IGuildMemberAddContext>(ctx =>
				ctx.Member.User.Id == new Snowflake(42) &&
				ctx.Guild.Id == new Snowflake(100)),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task GuildMemberRemove_InvokesIGuildMemberRemoveHandlerAsync()
	{
		var handler = Substitute.For<IGuildMemberRemoveHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.GuildMemberRemove(guildId: 100, userId: 42));

		await handler.Received(1).HandleAsync(
			Arg.Is<IGuildMemberRemoveContext>(ctx =>
				ctx.User.Id == new Snowflake(42) &&
				ctx.Guild.Id == new Snowflake(100)),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task GuildMemberUpdate_InvokesIGuildMemberUpdateHandlerAsync()
	{
		var handler = Substitute.For<IGuildMemberUpdateHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.GuildMemberUpdate(guildId: 100, userId: 42, nickname: "renamed"));

		await handler.Received(1).HandleAsync(
			Arg.Is<IGuildMemberUpdateContext>(ctx =>
				ctx.Member.User.Id == new Snowflake(42) &&
				ctx.Member.Nickname == "renamed"),
			Arg.Any<IServiceProvider>());
	}
}

using DiscoSdk.Contexts.Channels;
using DiscoSdk.Events;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using DiscoSdk.Models;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Gateway.Events;

public class ThreadDispatchTests : DispatcherTestBase
{
	[Fact]
	public async Task ThreadCreate_InvokesIThreadCreateHandlerAsync()
	{
		var handler = Substitute.For<IThreadCreateHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.ThreadCreate(id: 400, guildId: 100, parentId: 200));

		await handler.Received(1).HandleAsync(
			Arg.Is<IThreadContext>(ctx => ctx.Thread.Id == new Snowflake(400)),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task ThreadUpdate_InvokesIThreadUpdateHandlerAsync()
	{
		var handler = Substitute.For<IThreadUpdateHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.ThreadUpdate(id: 400, guildId: 100, name: "renamed"));

		await handler.Received(1).HandleAsync(
			Arg.Is<IThreadContext>(ctx => ctx.Thread.Id == new Snowflake(400)),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task ThreadDelete_InvokesIThreadDeleteHandlerAsync()
	{
		var handler = Substitute.For<IThreadDeleteHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.ThreadDelete(id: 400, guildId: 100, parentId: 200));

		await handler.Received(1).HandleAsync(
			Arg.Is<IThreadDeleteContext>(ctx => ctx.ThreadId == new Snowflake(400) && ctx.ParentId == new Snowflake(200)),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task ThreadListSync_InvokesIThreadListSyncHandlerAsync()
	{
		var handler = Substitute.For<IThreadListSyncHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.ThreadListSync(guildId: 100, threadIds: [401UL, 402UL]));

		await handler.Received(1).HandleAsync(
			Arg.Is<IThreadListSyncContext>(ctx => ctx.Threads.Length == 2),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task ThreadMemberUpdate_InvokesIThreadMemberUpdateHandlerAsync()
	{
		var handler = Substitute.For<IThreadMemberUpdateHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.ThreadMemberUpdate(threadId: 400, userId: 42, guildId: 100));

		await handler.Received(1).HandleAsync(
			Arg.Is<IThreadMemberUpdateContext>(ctx => ctx.ThreadId == new Snowflake(400) && ctx.UserId == new Snowflake(42)),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task ThreadMembersUpdate_InvokesIThreadMembersUpdateHandlerAsync()
	{
		var handler = Substitute.For<IThreadMembersUpdateHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.ThreadMembersUpdate(
			threadId: 400, guildId: 100, memberCount: 5,
			addedIds: [42UL, 43UL], removedIds: [44UL]));

		await handler.Received(1).HandleAsync(
			Arg.Is<IThreadMembersUpdateContext>(ctx =>
				ctx.ThreadId == new Snowflake(400) &&
				ctx.MemberCount == 5 &&
				ctx.AddedUserIds.Length == 2 &&
				ctx.RemovedUserIds.Length == 1),
			Arg.Any<IServiceProvider>());
	}
}

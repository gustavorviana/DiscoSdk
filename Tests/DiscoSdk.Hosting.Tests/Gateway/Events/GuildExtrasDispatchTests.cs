using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Events;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using DiscoSdk.Models;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Gateway.Events;

/// <summary>
/// AUDIT_LOG, EMOJIS/STICKERS/INTEGRATIONS_UPDATE, PRESENCE_UPDATE, GUILD_MEMBERS_CHUNK dispatch routes.
/// </summary>
public class GuildExtrasDispatchTests : DispatcherTestBase
{
	[Fact]
	public async Task AuditLogEntryCreate_InvokesIAuditLogEntryCreateHandlerAsync()
	{
		var handler = Substitute.For<IAuditLogEntryCreateHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.AuditLogEntryCreate(id: 1000, guildId: 100, actionType: 25));

		await handler.Received(1).HandleAsync(
			Arg.Is<IAuditLogEntryCreateContext>(ctx => ctx.Entry.Id == new Snowflake(1000)),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task GuildEmojisUpdate_InvokesIGuildEmojisUpdateHandlerAsync()
	{
		var handler = Substitute.For<IGuildEmojisUpdateHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.GuildEmojisUpdate(guildId: 100, emojis: [(1100UL, "smile"), (1101UL, "frown")]));

		await handler.Received(1).HandleAsync(
			Arg.Is<IGuildEmojisUpdateContext>(ctx => ctx.Emojis.Length == 2),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task GuildStickersUpdate_InvokesIGuildStickersUpdateHandlerAsync()
	{
		var handler = Substitute.For<IGuildStickersUpdateHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.GuildStickersUpdate(guildId: 100, stickers: [(1200UL, "fancy")]));

		await handler.Received(1).HandleAsync(
			Arg.Is<IGuildStickersUpdateContext>(ctx => ctx.Stickers.Length == 1 && ctx.Stickers[0].Name == "fancy"),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task GuildIntegrationsUpdate_InvokesIGuildIntegrationsUpdateHandlerAsync()
	{
		var handler = Substitute.For<IGuildIntegrationsUpdateHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.GuildIntegrationsUpdate(guildId: 100));

		await handler.Received(1).HandleAsync(
			Arg.Is<IGuildIntegrationsUpdateContext>(ctx => ctx.Guild.Id == new Snowflake(100)),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task PresenceUpdate_InvokesIPresenceUpdateHandlerAsync()
	{
		var handler = Substitute.For<IPresenceUpdateHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.PresenceUpdate(userId: 42, guildId: 100, status: "dnd"));

		await handler.Received(1).HandleAsync(
			Arg.Is<IPresenceUpdateContext>(ctx => ctx.Presence.Status == "dnd"),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task GuildMembersChunk_InvokesIGuildMembersChunkHandlerAsync()
	{
		var handler = Substitute.For<IGuildMembersChunkHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.GuildMembersChunk(
			guildId: 100, chunkIndex: 0, chunkCount: 2, nonce: "req-1", userIds: [42UL, 43UL]));

		await handler.Received(1).HandleAsync(
			Arg.Is<IGuildMembersChunkContext>(ctx =>
				ctx.Members.Length == 2 &&
				ctx.ChunkIndex == 0 &&
				ctx.ChunkCount == 2 &&
				ctx.Nonce == "req-1"),
			Arg.Any<IServiceProvider>());
	}
}

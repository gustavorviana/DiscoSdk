using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers.Channels;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Channels;

public class GuildThreadChannelWrapperTests : WrapperTestBase
{
	private readonly IGuild _guild;

	public GuildThreadChannelWrapperTests()
	{
		_guild = Substitute.For<IGuild>();
		_guild.Id.Returns(new Snowflake(100));
	}

	private GuildThreadChannelWrapper NewWrapper(Snowflake? parentId = null)
		=> new(Client, new Channel { Id = new Snowflake(200), Type = ChannelType.PublicThread, ParentId = parentId }, _guild);

	[Fact]
	public async Task JoinThread_PutsThreadMembersMeAsync()
	{
		await NewWrapper().JoinThread().ExecuteAsync();
		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200/thread-members/@me"),
			HttpMethod.Put, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task LeaveThread_DeletesThreadMembersMeAsync()
	{
		await NewWrapper().LeaveThread().ExecuteAsync();
		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200/thread-members/@me"),
			HttpMethod.Delete, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task AddThreadMember_PutsThreadMemberByIdAsync()
	{
		await NewWrapper().AddThreadMember(new Snowflake(42)).ExecuteAsync();
		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200/thread-members/42"),
			HttpMethod.Put, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task RemoveThreadMember_DeletesThreadMemberByIdAsync()
	{
		await NewWrapper().RemoveThreadMember(new Snowflake(42)).ExecuteAsync();
		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200/thread-members/42"),
			HttpMethod.Delete, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ArchiveThread_PatchesChannelArchivedTrueAsync()
	{
		await NewWrapper().ArchiveThread().ExecuteAsync();
		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200"),
			HttpMethod.Patch,
			Arg.Is<object?>(b => BodyPropertyEquals(b, "archived", true)),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task UnarchiveThread_PatchesChannelArchivedFalseAsync()
	{
		await NewWrapper().UnarchiveThread().ExecuteAsync();
		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200"),
			HttpMethod.Patch,
			Arg.Is<object?>(b => BodyPropertyEquals(b, "archived", false)),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task LockThread_PatchesChannelLockedTrueAsync()
	{
		await NewWrapper().LockThread().ExecuteAsync();
		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200"),
			HttpMethod.Patch,
			Arg.Is<object?>(b => BodyPropertyEquals(b, "locked", true)),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task UnlockThread_PatchesChannelLockedFalseAsync()
	{
		await NewWrapper().UnlockThread().ExecuteAsync();
		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200"),
			HttpMethod.Patch,
			Arg.Is<object?>(b => BodyPropertyEquals(b, "locked", false)),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetParentChannelId_FetchesParentChannelAsync()
	{
		Http.SendAsync<Channel>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Channel { Id = new Snowflake(999), Type = ChannelType.GuildText });

		await NewWrapper(parentId: new Snowflake(999)).GetParentChannelId().ExecuteAsync();

		await Http.Received(1).SendAsync<Channel>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/999"),
			HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}
}

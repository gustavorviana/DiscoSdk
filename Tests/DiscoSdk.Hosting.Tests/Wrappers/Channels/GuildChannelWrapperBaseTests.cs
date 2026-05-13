using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers.Channels;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Channels;

/// <summary>
/// Tests the actions exposed by <see cref="GuildChannelWrapperBase"/> through a concrete subclass
/// (<see cref="GuildTextChannelWrapper"/>), since the base type is abstract.
/// </summary>
public class GuildChannelWrapperBaseTests : WrapperTestBase
{
	private readonly IGuild _guild;

	public GuildChannelWrapperBaseTests()
	{
		_guild = Substitute.For<IGuild>();
		_guild.Id.Returns(new Snowflake(100));
	}

	private GuildTextChannelWrapper NewWrapper()
		=> new(Client, new Channel { Id = new Snowflake(200), Type = ChannelType.GuildText }, _guild);

	[Fact]
	public async Task CreateInvite_PostsChannelInvitesRouteAsync()
	{
		Http.SendAsync<Invite>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Invite { Code = "x" });
		var wrapper = NewWrapper();

		await wrapper.CreateInvite().SetMaxAge(60).ExecuteAsync();

		await Http.Received(1).SendAsync<Invite>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200/invites"),
			HttpMethod.Post, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task RetrieveInvites_GetsChannelInvitesRouteAsync()
	{
		Http.SendAsync<Invite[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);
		var wrapper = NewWrapper();

		await wrapper.RetrieveInvites().ExecuteAsync();

		await Http.Received(1).SendAsync<Invite[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200/invites"),
			HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}
}

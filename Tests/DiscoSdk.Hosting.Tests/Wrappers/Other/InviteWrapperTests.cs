using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Other;

public class InviteWrapperTests : WrapperTestBase
{
	[Fact]
	public async Task Delete_DeletesInviteByCodeAsync()
	{
		var channel = Substitute.For<IGuildChannelBase>();
		channel.Guild.Returns(Substitute.For<IGuild>());
		var wrapper = new InviteWrapper(new Invite { Code = "abc" }, channel, Client);

		await wrapper.Delete().ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "invites/abc"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}
}

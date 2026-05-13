using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Users;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Other;

public class UserWrapperTests : WrapperTestBase
{
	[Fact]
	public async Task OpenPrivateChannel_PostsCreateDmRouteAsync()
	{
		Http.SendAsync<Channel>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Channel { Id = new Snowflake(500) });

		var wrapper = new UserWrapper(Client, new User { UserId = new Snowflake(42), Username = "u" });

		await wrapper.OpenPrivateChannel().ExecuteAsync();

		// DmRepository.OpenDm → ChannelClient.CreateDMAsync → POST users/@me/channels with recipient_id=42.
		await Http.Received(1).SendAsync<Channel>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "users/@me/channels"),
			HttpMethod.Post,
			Arg.Is<object?>(b => BodyPropertyEquals(b, "recipient_id", "42")),
			Arg.Any<CancellationToken>());
	}
}

using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers.Managers;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Rest;
using DiscoSdk.Rest.Actions;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Managers;

/// <summary>
/// Verifies that <see cref="GuildChannelManagerWrapper"/> — which only exposes the shared
/// <see cref="ChannelManagerWrapper{TSelf}"/> setters — turns staged changes into the right
/// <c>PATCH /channels/{id}</c> call when executed.
/// </summary>
public class GuildChannelManagerWrapperTests : WrapperTestBase
{
	[Fact]
	public async Task ExecuteAsync_PatchesChannelWithName_PositionAndParentAsync()
	{
		Http.SendAsync<Channel>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Channel());
		var mgr = new GuildChannelManagerWrapper(new Snowflake(200), Client.ChannelClient);

		mgr.SetName("renamed");
		mgr.SetPosition(3);
		mgr.SetParent(new Snowflake(50));
		await ((IRestAction)mgr).ExecuteAsync();

		await Http.Received(1).SendAsync<Channel>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200"),
			HttpMethod.Patch,
			Arg.Is<object?>(body =>
				BodyContains(body, "name", "renamed") &&
				BodyContains(body, "position", 3) &&
				BodyContains(body, "parent_id", "50")),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ExecuteAsync_NoChanges_DoesNotCallApiAsync()
	{
		var mgr = new GuildChannelManagerWrapper(new Snowflake(200), Client.ChannelClient);

		await ((IRestAction)mgr).ExecuteAsync();

		await Http.DidNotReceiveWithAnyArgs().SendAsync<Channel>(default, default!, default, default);
	}
}

using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers.Channels;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Channels;

public class ChannelWrapperTests : WrapperTestBase
{
	[Fact]
	public async Task Delete_DeletesChannelAsync()
	{
		var wrapper = new ChannelWrapper(Client, new Channel { Id = new Snowflake(42) });

		await wrapper.Delete().ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/42"),
			HttpMethod.Delete, Arg.Any<CancellationToken>());
	}
}

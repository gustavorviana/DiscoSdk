using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers.Channels;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Channels;

public class DmChannelWrapperTests : WrapperTestBase
{
	[Fact]
	public async Task Delete_DeletesUnderlyingChannelAsync()
	{
		var wrapper = new DmChannelWrapper(Client, new Channel { Id = new Snowflake(42) });

		await wrapper.Delete().ExecuteAsync();

		// Delegates to the base ChannelWrapper.Delete() — DELETE /channels/{id}.
		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/42"),
			HttpMethod.Delete, Arg.Any<CancellationToken>());
	}
}

using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Other;

public class IntegrationWrapperTests : WrapperTestBase
{
	[Fact]
	public async Task Delete_DeletesGuildIntegrationAsync()
	{
		var wrapper = new IntegrationWrapper(Client, guildId: new Snowflake(100), new Integration
		{
			Id = new Snowflake(42),
			Account = new IntegrationAccount(),
		});

		await wrapper.Delete().ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/integrations/42"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}
}

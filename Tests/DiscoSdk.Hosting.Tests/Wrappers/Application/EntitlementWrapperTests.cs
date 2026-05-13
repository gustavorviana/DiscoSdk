using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Monetization;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Application;

public class EntitlementWrapperTests : WrapperTestBase
{
	private static Entitlement Model() => new()
	{
		Id = new Snowflake(42),
		SkuId = new Snowflake(11),
		ApplicationId = new Snowflake(100),
	};

	[Fact]
	public async Task Consume_PostsConsumeRouteAsync()
	{
		var wrapper = new EntitlementWrapper(Client, Model());

		await wrapper.Consume().ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "applications/100/entitlements/42/consume"),
			HttpMethod.Post,
			Arg.Is<object?>(b => b == null),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task DeleteTest_DeletesEntitlementAsync()
	{
		var wrapper = new EntitlementWrapper(Client, Model());

		await wrapper.DeleteTest().ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "applications/100/entitlements/42"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}
}

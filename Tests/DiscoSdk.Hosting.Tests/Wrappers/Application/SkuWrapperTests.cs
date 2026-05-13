using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Monetization;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Application;

public class SkuWrapperTests : WrapperTestBase
{
	private static Sku Model() => new() { Id = new Snowflake(42) };

	[Fact]
	public async Task GetSubscriptions_NoUser_GetsListRouteAsync()
	{
		Http.SendAsync<Subscription[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);
		var wrapper = new SkuWrapper(Client, Model());

		await wrapper.GetSubscriptions().ExecuteAsync();

		await Http.Received(1).SendAsync<Subscription[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "skus/42/subscriptions"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetSubscriptions_WithUser_AppendsUserIdQueryAsync()
	{
		Http.SendAsync<Subscription[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);
		var wrapper = new SkuWrapper(Client, Model());

		await wrapper.GetSubscriptions(new Snowflake(77)).ExecuteAsync();

		await Http.Received(1).SendAsync<Subscription[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "skus/42/subscriptions?user_id=77"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetSubscription_GetsSingleSubscriptionRouteAsync()
	{
		Http.SendAsync<Subscription>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Subscription());
		var wrapper = new SkuWrapper(Client, Model());

		await wrapper.GetSubscription(new Snowflake(99)).ExecuteAsync();

		await Http.Received(1).SendAsync<Subscription>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "skus/42/subscriptions/99"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}
}

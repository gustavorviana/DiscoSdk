using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Models.Applications;
using DiscoSdk.Models.Monetization;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Rest.Clients;

public class ApplicationClientTests
{
	private readonly IDiscordRestClient _http = Substitute.For<IDiscordRestClient>();
	private readonly ApplicationClient _client;
	private readonly Snowflake _appId = new(1);
	private readonly Snowflake _skuId = new(2);
	private readonly Snowflake _entitlementId = new(3);
	private readonly Snowflake _subscriptionId = new(4);

	public ApplicationClientTests()
	{
		_client = new ApplicationClient(_http);
	}

	[Fact]
	public async Task GetCurrentApplicationAsync_GetsMeApplicationAsync()
	{
		_http.SendAsync<Application>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Application());

		await _client.GetCurrentApplicationAsync();

		await _http.Received(1).SendAsync<Application>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "applications/@me"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task EditCurrentApplicationAsync_PatchesMeApplicationAsync()
	{
		_http.SendAsync<Application>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Application());

		var req = new { description = "x" };
		await _client.EditCurrentApplicationAsync(req);

		await _http.Received(1).SendAsync<Application>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "applications/@me"),
			HttpMethod.Patch,
			req,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetRoleConnectionMetadataAsync_GetsRoleConnectionMetadataRouteAsync()
	{
		_http.SendAsync<ApplicationRoleConnectionMetadata[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _client.GetRoleConnectionMetadataAsync(_appId);

		await _http.Received(1).SendAsync<ApplicationRoleConnectionMetadata[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/role-connections/metadata"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task UpdateRoleConnectionMetadataAsync_PutsRoleConnectionMetadataRouteAsync()
	{
		_http.SendAsync<ApplicationRoleConnectionMetadata[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		var records = new[] { new ApplicationRoleConnectionMetadata { Key = "level", Name = "Level", Description = "lvl" } };
		await _client.UpdateRoleConnectionMetadataAsync(_appId, records);

		await _http.Received(1).SendAsync<ApplicationRoleConnectionMetadata[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/role-connections/metadata"),
			HttpMethod.Put,
			records,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ListSkusAsync_GetsSkusRouteAsync()
	{
		_http.SendAsync<Sku[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _client.ListSkusAsync(_appId);

		await _http.Received(1).SendAsync<Sku[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/skus"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ListEntitlementsAsync_NoFilters_HasNoQueryAsync()
	{
		_http.SendAsync<Entitlement[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _client.ListEntitlementsAsync(_appId);

		await _http.Received(1).SendAsync<Entitlement[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/entitlements"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ListEntitlementsAsync_AppendsQueryFiltersAsync()
	{
		_http.SendAsync<Entitlement[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _client.ListEntitlementsAsync(_appId, userId: new Snowflake(55), guildId: new Snowflake(66), excludeEnded: true, excludeDeleted: false);

		await _http.Received(1).SendAsync<Entitlement[]>(
			Arg.Is<DiscordRoute>(r =>
				r.ToString().StartsWith($"applications/{_appId}/entitlements?") &&
				r.ToString().Contains("user_id=55") &&
				r.ToString().Contains("guild_id=66") &&
				r.ToString().Contains("exclude_ended=true") &&
				r.ToString().Contains("exclude_deleted=false")),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetEntitlementAsync_GetsByIdRouteAsync()
	{
		_http.SendAsync<Entitlement>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Entitlement());

		await _client.GetEntitlementAsync(_appId, _entitlementId);

		await _http.Received(1).SendAsync<Entitlement>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/entitlements/{_entitlementId}"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateTestEntitlementAsync_PostsBodyAsync()
	{
		_http.SendAsync<Entitlement>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Entitlement());

		await _client.CreateTestEntitlementAsync(_appId, _skuId, new Snowflake(777), ownerType: 2);

		await _http.Received(1).SendAsync<Entitlement>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/entitlements"),
			HttpMethod.Post,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task DeleteTestEntitlementAsync_DeletesByIdRouteAsync()
	{
		await _client.DeleteTestEntitlementAsync(_appId, _entitlementId);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/entitlements/{_entitlementId}"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ConsumeEntitlementAsync_PostsConsumeRouteAsync()
	{
		await _client.ConsumeEntitlementAsync(_appId, _entitlementId);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/entitlements/{_entitlementId}/consume"),
			HttpMethod.Post,
			Arg.Is<object?>(b => b == null),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ListSkuSubscriptionsAsync_OmitsQueryWhenNoUserIdAsync()
	{
		_http.SendAsync<Subscription[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _client.ListSkuSubscriptionsAsync(_skuId);

		await _http.Received(1).SendAsync<Subscription[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"skus/{_skuId}/subscriptions"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ListSkuSubscriptionsAsync_AppendsUserIdQueryAsync()
	{
		_http.SendAsync<Subscription[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _client.ListSkuSubscriptionsAsync(_skuId, userId: new Snowflake(42));

		await _http.Received(1).SendAsync<Subscription[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"skus/{_skuId}/subscriptions?user_id=42"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetSkuSubscriptionAsync_BuildsRouteAsync()
	{
		_http.SendAsync<Subscription>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Subscription());

		await _client.GetSkuSubscriptionAsync(_skuId, _subscriptionId);

		await _http.Received(1).SendAsync<Subscription>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"skus/{_skuId}/subscriptions/{_subscriptionId}"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}
}

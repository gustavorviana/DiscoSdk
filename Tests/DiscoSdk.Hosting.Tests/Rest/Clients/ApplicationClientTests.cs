using DiscoSdk.Exceptions;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Models.Applications;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Monetization;
using DiscoSdk.Rest;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Net;

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

	// ---- Activity instances ----

	[Fact]
	public async Task GetActivityInstanceAsync_GetsInstanceRouteAsync()
	{
		var instance = new ActivityInstance
		{
			ApplicationId = _appId,
			InstanceId = "i.abc",
			LaunchId = "789",
			Location = new ActivityInstanceLocation
			{
				Id = "loc-1",
				Kind = ActivityLocationKind.GuildChannel,
				ChannelId = new Snowflake(42),
				GuildId = new Snowflake(100),
			},
			Users = [new Snowflake(7), new Snowflake(8)],
		};
		_http.SendAsync<ActivityInstance>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(instance);

		var result = await _client.GetActivityInstanceAsync(_appId, "i.abc");

		Assert.NotNull(result);
		Assert.Equal("i.abc", result!.InstanceId);
		Assert.Equal(ActivityLocationKind.GuildChannel, result.Location!.Kind);
		Assert.Equal(2, result.Users.Length);

		await _http.Received(1).SendAsync<ActivityInstance>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/activity-instances/i.abc"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetActivityInstanceAsync_OnUnknownInstance_ReturnsNullAsync()
	{
		// Discord 404 surfaces as DiscordResourceNotFoundException (10068 Unknown Voice State /
		// similar Unknown X family). The endpoint maps it to null.
		var notFound = new DiscordResourceNotFoundExceptionShim(
			HttpStatusCode.NotFound,
			"Not Found",
			new DiscordApiError { Code = 10003, Message = "Unknown Channel" });
		_http.SendAsync<ActivityInstance>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Throws(notFound);

		var result = await _client.GetActivityInstanceAsync(_appId, "i.gone");

		Assert.Null(result);
	}

	[Fact]
	public async Task GetActivityInstanceAsync_OnGenericNotFound_ReturnsNullAsync()
	{
		// Defensive branch: 404 without a typed JSON error code (no DiscordResourceNotFoundException)
		// still resolves to null.
		var rawNotFound = new DiscordApiException(HttpStatusCode.NotFound, "Not Found", error: null);
		_http.SendAsync<ActivityInstance>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Throws(rawNotFound);

		var result = await _client.GetActivityInstanceAsync(_appId, "i.gone");

		Assert.Null(result);
	}

	[Fact]
	public async Task GetActivityInstanceAsync_OnOtherFailure_PropagatesAsync()
	{
		var permError = new DiscordApiException(
			HttpStatusCode.Forbidden,
			"Forbidden",
			new DiscordApiError { Code = 50013, Message = "Missing Permissions" });
		_http.SendAsync<ActivityInstance>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Throws(permError);

		await Assert.ThrowsAsync<DiscordApiException>(() =>
			_client.GetActivityInstanceAsync(_appId, "i.abc"));
	}

	[Fact]
	public async Task GetActivityInstanceAsync_RejectsBlankInstanceIdAsync()
	{
		await Assert.ThrowsAsync<ArgumentException>(() =>
			_client.GetActivityInstanceAsync(_appId, "   "));
	}

	/// <summary>
	/// Test shim around <see cref="DiscordResourceNotFoundException"/> which has an internal
	/// constructor. Subclassing here lets the test simulate the exact subtype the production
	/// REST client raises for 404 + Unknown X codes.
	/// </summary>
	private sealed class DiscordResourceNotFoundExceptionShim : DiscordResourceNotFoundException
	{
		internal DiscordResourceNotFoundExceptionShim(HttpStatusCode statusCode, string? httpReasonPhrase, DiscordApiError error)
			: base(statusCode, httpReasonPhrase, error)
		{
		}
	}
}

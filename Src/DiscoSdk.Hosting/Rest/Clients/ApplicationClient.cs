using DiscoSdk.Exceptions;
using DiscoSdk.Models;
using DiscoSdk.Models.Applications;
using DiscoSdk.Models.Monetization;
using DiscoSdk.Rest;
using System.Net;
using System.Text;

namespace DiscoSdk.Hosting.Rest.Clients;

/// <summary>
/// Client for application-scoped Discord operations: the current application object, role-connection
/// metadata records, SKUs, entitlements and subscriptions.
/// </summary>
/// <param name="client">The REST client base to use for requests.</param>
internal class ApplicationClient(IDiscordRestClient client)
{
	// ---- Application ----

	/// <summary>Gets the application object associated with the requesting bot.</summary>
	public Task<Application> GetCurrentApplicationAsync(CancellationToken cancellationToken = default)
	{
		var route = new DiscordRoute("applications/@me");
		return client.SendAsync<Application>(route, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>Edits properties of the current application.</summary>
	public Task<Application> EditCurrentApplicationAsync(object request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		var route = new DiscordRoute("applications/@me");
		return client.SendAsync<Application>(route, HttpMethod.Patch, request, cancellationToken);
	}

	// ---- Role connection metadata ----

	/// <summary>Gets the role-connection metadata records for the application.</summary>
	public Task<ApplicationRoleConnectionMetadata[]> GetRoleConnectionMetadataAsync(Snowflake applicationId, CancellationToken cancellationToken = default)
	{
		var route = new DiscordRoute("applications/{application_id}/role-connections/metadata", applicationId);
		return client.SendAsync<ApplicationRoleConnectionMetadata[]>(route, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>Replaces the role-connection metadata records for the application (max 5).</summary>
	public Task<ApplicationRoleConnectionMetadata[]> UpdateRoleConnectionMetadataAsync(Snowflake applicationId, IEnumerable<ApplicationRoleConnectionMetadata> records, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(records);

		var route = new DiscordRoute("applications/{application_id}/role-connections/metadata", applicationId);
		return client.SendAsync<ApplicationRoleConnectionMetadata[]>(route, HttpMethod.Put, records, cancellationToken);
	}

	// ---- SKUs ----

	/// <summary>Lists the SKUs for the application.</summary>
	public Task<Sku[]> ListSkusAsync(Snowflake applicationId, CancellationToken cancellationToken = default)
	{
		var route = new DiscordRoute("applications/{application_id}/skus", applicationId);
		return client.SendAsync<Sku[]>(route, HttpMethod.Get, null, cancellationToken);
	}

	// ---- Entitlements ----

	/// <summary>Lists entitlements for the application, optionally filtered by user and/or guild.</summary>
	public Task<Entitlement[]> ListEntitlementsAsync(Snowflake applicationId, Snowflake? userId = null, Snowflake? guildId = null, bool? excludeEnded = null, bool? excludeDeleted = null, CancellationToken cancellationToken = default)
	{
		var path = new StringBuilder("applications/{application_id}/entitlements");
		var hasQuery = false;

		void Append(string key, string value)
		{
			path.Append(hasQuery ? '&' : '?').Append(key).Append('=').Append(Uri.EscapeDataString(value));
			hasQuery = true;
		}

		if (userId is { } u) Append("user_id", u.ToString());
		if (guildId is { } g) Append("guild_id", g.ToString());
		if (excludeEnded is { } ee) Append("exclude_ended", ee ? "true" : "false");
		if (excludeDeleted is { } ed) Append("exclude_deleted", ed ? "true" : "false");

		var route = new DiscordRoute(path.ToString(), applicationId);
		return client.SendAsync<Entitlement[]>(route, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>Gets a single entitlement.</summary>
	public Task<Entitlement> GetEntitlementAsync(Snowflake applicationId, Snowflake entitlementId, CancellationToken cancellationToken = default)
	{
		var route = new DiscordRoute("applications/{application_id}/entitlements/{entitlement_id}", applicationId, entitlementId);
		return client.SendAsync<Entitlement>(route, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>
	/// Creates a test entitlement (no payment) for a given SKU and owner.
	/// </summary>
	/// <param name="ownerType">1 for a guild subscription, 2 for a user subscription.</param>
	public Task<Entitlement> CreateTestEntitlementAsync(Snowflake applicationId, Snowflake skuId, Snowflake ownerId, int ownerType, CancellationToken cancellationToken = default)
	{
		var route = new DiscordRoute("applications/{application_id}/entitlements", applicationId);
		var request = new { sku_id = skuId.ToString(), owner_id = ownerId.ToString(), owner_type = ownerType };
		return client.SendAsync<Entitlement>(route, HttpMethod.Post, request, cancellationToken);
	}

	/// <summary>Deletes a test entitlement.</summary>
	public Task DeleteTestEntitlementAsync(Snowflake applicationId, Snowflake entitlementId, CancellationToken cancellationToken = default)
	{
		var route = new DiscordRoute("applications/{application_id}/entitlements/{entitlement_id}", applicationId, entitlementId);
		return client.SendAsync(route, HttpMethod.Delete, cancellationToken);
	}

	/// <summary>Marks a one-time-purchase consumable entitlement as consumed.</summary>
	public Task ConsumeEntitlementAsync(Snowflake applicationId, Snowflake entitlementId, CancellationToken cancellationToken = default)
	{
		var route = new DiscordRoute("applications/{application_id}/entitlements/{entitlement_id}/consume", applicationId, entitlementId);
		return client.SendAsync(route, HttpMethod.Post, null, cancellationToken);
	}

	// ---- Subscriptions ----

	/// <summary>Lists subscriptions for an SKU.</summary>
	public Task<Subscription[]> ListSkuSubscriptionsAsync(Snowflake skuId, Snowflake? userId = null, CancellationToken cancellationToken = default)
	{
		var path = new StringBuilder("skus/{sku_id}/subscriptions");
		if (userId is { } u)
			path.Append("?user_id=").Append(Uri.EscapeDataString(u.ToString()));

		var route = new DiscordRoute(path.ToString(), skuId);
		return client.SendAsync<Subscription[]>(route, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>Gets a single subscription for an SKU.</summary>
	public Task<Subscription> GetSkuSubscriptionAsync(Snowflake skuId, Snowflake subscriptionId, CancellationToken cancellationToken = default)
	{
		var route = new DiscordRoute("skus/{sku_id}/subscriptions/{subscription_id}", skuId, subscriptionId);
		return client.SendAsync<Subscription>(route, HttpMethod.Get, null, cancellationToken);
	}

	// ---- Activity instances ----

	/// <summary>
	/// Gets a running embedded-activity instance for the application. Returns <c>null</c>
	/// when Discord answers with HTTP 404 (instance never existed, or was already torn down).
	/// </summary>
	/// <remarks>
	/// Endpoint: <c>GET /applications/{application_id}/activity-instances/{instance_id}</c>.
	/// Docs: <see href="https://discord.com/developers/docs/resources/application#get-application-activity-instance"/>.
	/// </remarks>
	public async Task<ActivityInstance?> GetActivityInstanceAsync(Snowflake applicationId, string instanceId, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(instanceId))
			throw new ArgumentException("Instance ID cannot be null or empty.", nameof(instanceId));

		var route = new DiscordRoute("applications/{application_id}/activity-instances/{instance_id}", applicationId, instanceId);
		try
		{
			return await client.SendAsync<ActivityInstance>(route, HttpMethod.Get, null, cancellationToken);
		}
		catch (DiscordResourceNotFoundException)
		{
			return null;
		}
		catch (DiscordApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
		{
			// Defensive: 404 without one of the Unknown X JSON codes still means "not found".
			return null;
		}
	}

	// ---- Application emojis ----

	/// <summary>Lists application-owned emojis. Response is wrapped in <see cref="ApplicationEmojiList.Items"/>.</summary>
	public Task<ApplicationEmojiList> ListApplicationEmojisAsync(Snowflake applicationId, CancellationToken cancellationToken = default)
	{
		var route = new DiscordRoute("applications/{application_id}/emojis", applicationId);
		return client.SendAsync<ApplicationEmojiList>(route, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>Gets a single application-owned emoji.</summary>
	public Task<InternalEmoji> GetApplicationEmojiAsync(Snowflake applicationId, Snowflake emojiId, CancellationToken cancellationToken = default)
	{
		var route = new DiscordRoute("applications/{application_id}/emojis/{emoji_id}", applicationId, emojiId);
		return client.SendAsync<InternalEmoji>(route, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>Creates an application-owned emoji.</summary>
	public Task<InternalEmoji> CreateApplicationEmojiAsync(Snowflake applicationId, object request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		var route = new DiscordRoute("applications/{application_id}/emojis", applicationId);
		return client.SendAsync<InternalEmoji>(route, HttpMethod.Post, request, cancellationToken);
	}

	/// <summary>Modifies an application-owned emoji (only the name can change).</summary>
	public Task<InternalEmoji> ModifyApplicationEmojiAsync(Snowflake applicationId, Snowflake emojiId, object request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		var route = new DiscordRoute("applications/{application_id}/emojis/{emoji_id}", applicationId, emojiId);
		return client.SendAsync<InternalEmoji>(route, HttpMethod.Patch, request, cancellationToken);
	}

	/// <summary>Deletes an application-owned emoji.</summary>
	public Task DeleteApplicationEmojiAsync(Snowflake applicationId, Snowflake emojiId, CancellationToken cancellationToken = default)
	{
		var route = new DiscordRoute("applications/{application_id}/emojis/{emoji_id}", applicationId, emojiId);
		return client.SendAsync(route, HttpMethod.Delete, cancellationToken);
	}
}

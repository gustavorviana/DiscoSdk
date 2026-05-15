using DiscoSdk.Exceptions;
using DiscoSdk.Models;
using DiscoSdk.Rest;

namespace DiscoSdk.Hosting.Rest.Clients;

/// <summary>
/// Client for the channel/guild-scoped Discord webhook endpoints (by id, with or without token).
/// </summary>
/// <remarks>
/// Distinct from <see cref="WebhookMessageClient"/>, which covers the <em>token-based</em> webhook
/// execution surface (send / edit / delete message via id+token).
/// </remarks>
internal sealed class WebhookClient(IDiscordRestClient client)
{
	/// <summary>Creates a new webhook on a guild channel.</summary>
	public Task<Webhook> CreateAsync(Snowflake channelId, string name, string? avatar = null, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Webhook name cannot be null or empty.", nameof(name));

		var body = avatar is null ? (object)new { name } : new { name, avatar };
		var route = new DiscordRoute("channels/{channel_id}/webhooks", channelId);
		return client.SendAsync<Webhook>(route, HttpMethod.Post, body, cancellationToken);
	}

	/// <summary>Lists all webhooks on a channel.</summary>
	public Task<Webhook[]> GetChannelWebhooksAsync(Snowflake channelId, CancellationToken cancellationToken = default)
	{
		var route = new DiscordRoute("channels/{channel_id}/webhooks", channelId);
		return client.SendAsync<Webhook[]>(route, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>Lists all webhooks across all channels in a guild.</summary>
	public Task<Webhook[]> GetGuildWebhooksAsync(Snowflake guildId, CancellationToken cancellationToken = default)
	{
		var route = new DiscordRoute("guilds/{guild_id}/webhooks", guildId);
		return client.SendAsync<Webhook[]>(route, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>Gets a webhook by ID. Returns <c>null</c> if not found.</summary>
	public async Task<Webhook?> GetAsync(Snowflake webhookId, CancellationToken cancellationToken = default)
	{
		var route = new DiscordRoute("webhooks/{webhook_id}", webhookId);
		try
		{
			return await client.SendAsync<Webhook>(route, HttpMethod.Get, null, cancellationToken);
		}
		catch (DiscordApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	/// <summary>
	/// Gets a webhook by ID and token. This variant does not require the bot to have permissions on
	/// the channel/guild. Returns <c>null</c> if not found.
	/// </summary>
	public async Task<Webhook?> GetWithTokenAsync(Snowflake webhookId, string token, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(token))
			throw new ArgumentException("Webhook token cannot be null or empty.", nameof(token));

		var route = new DiscordRoute("webhooks/{webhook_id}/{webhook_token}", webhookId, token);
		try
		{
			return await client.SendAsync<Webhook>(route, HttpMethod.Get, null, cancellationToken);
		}
		catch (DiscordApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	/// <summary>Modifies a webhook with a preconstructed body (used by the builder action).</summary>
	public Task<Webhook> ModifyAsync(Snowflake webhookId, IDictionary<string, object?> body, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(body);
		var route = new DiscordRoute("webhooks/{webhook_id}", webhookId);
		return client.SendAsync<Webhook>(route, HttpMethod.Patch, body, cancellationToken);
	}

	/// <summary>
	/// Modifies a webhook using its token. Does not require the bot to have permissions; <c>channel_id</c>
	/// cannot be updated via this variant.
	/// </summary>
	public Task<Webhook> ModifyWithTokenAsync(Snowflake webhookId, string token, string? name = null, string? avatar = null, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(token))
			throw new ArgumentException("Webhook token cannot be null or empty.", nameof(token));

		var body = new Dictionary<string, object?>();
		if (name is not null) body["name"] = name;
		if (avatar is not null) body["avatar"] = avatar;

		var route = new DiscordRoute("webhooks/{webhook_id}/{webhook_token}", webhookId, token);
		return client.SendAsync<Webhook>(route, HttpMethod.Patch, body, cancellationToken);
	}

	/// <summary>Deletes a webhook permanently.</summary>
	public Task DeleteAsync(Snowflake webhookId, CancellationToken cancellationToken = default)
	{
		var route = new DiscordRoute("webhooks/{webhook_id}", webhookId);
		return client.SendAsync(route, HttpMethod.Delete, cancellationToken);
	}

	/// <summary>Deletes a webhook permanently using its token (no permission check).</summary>
	public Task DeleteWithTokenAsync(Snowflake webhookId, string token, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(token))
			throw new ArgumentException("Webhook token cannot be null or empty.", nameof(token));

		var route = new DiscordRoute("webhooks/{webhook_id}/{webhook_token}", webhookId, token);
		return client.SendAsync(route, HttpMethod.Delete, cancellationToken);
	}
}

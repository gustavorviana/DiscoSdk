using DiscoSdk.Models;
using DiscoSdk.Rest;
using System.Text;

namespace DiscoSdk.Hosting.Rest.Clients;

/// <summary>
/// Client for the Discord Guild Scheduled Event REST surface.
/// </summary>
internal class GuildScheduledEventClient(IDiscordRestClient client)
{
	/// <summary>Lists the scheduled events in a guild.</summary>
	public Task<GuildScheduledEvent[]> ListAsync(Snowflake guildId, bool? withUserCount = null, CancellationToken cancellationToken = default)
	{
		var path = new StringBuilder("guilds/{guild_id}/scheduled-events");
		if (withUserCount.HasValue)
			path.Append("?with_user_count=").Append(withUserCount.Value ? "true" : "false");

		var route = new DiscordRoute(path.ToString(), guildId);
		return client.SendAsync<GuildScheduledEvent[]>(route, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>Creates a scheduled event in a guild.</summary>
	public Task<GuildScheduledEvent> CreateAsync(Snowflake guildId, object request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		var route = new DiscordRoute("guilds/{guild_id}/scheduled-events", guildId);
		return client.SendAsync<GuildScheduledEvent>(route, HttpMethod.Post, request, cancellationToken);
	}

	/// <summary>Gets a single scheduled event.</summary>
	public Task<GuildScheduledEvent> GetAsync(Snowflake guildId, Snowflake eventId, bool? withUserCount = null, CancellationToken cancellationToken = default)
	{
		var path = new StringBuilder("guilds/{guild_id}/scheduled-events/{event_id}");
		if (withUserCount.HasValue)
			path.Append("?with_user_count=").Append(withUserCount.Value ? "true" : "false");

		var route = new DiscordRoute(path.ToString(), guildId, eventId);
		return client.SendAsync<GuildScheduledEvent>(route, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>Modifies a scheduled event.</summary>
	public Task<GuildScheduledEvent> ModifyAsync(Snowflake guildId, Snowflake eventId, object request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		var route = new DiscordRoute("guilds/{guild_id}/scheduled-events/{event_id}", guildId, eventId);
		return client.SendAsync<GuildScheduledEvent>(route, HttpMethod.Patch, request, cancellationToken);
	}

	/// <summary>Deletes a scheduled event.</summary>
	public Task DeleteAsync(Snowflake guildId, Snowflake eventId, CancellationToken cancellationToken = default)
	{
		var route = new DiscordRoute("guilds/{guild_id}/scheduled-events/{event_id}", guildId, eventId);
		return client.SendAsync(route, HttpMethod.Delete, cancellationToken);
	}

	/// <summary>
	/// Gets users that subscribed to a scheduled event. Pagination via
	/// <paramref name="before"/>/<paramref name="after"/> (user id keyset).
	/// </summary>
	public Task<GuildScheduledEventUser[]> GetUsersAsync(
		Snowflake guildId,
		Snowflake eventId,
		int? limit = null,
		bool? withMember = null,
		Snowflake? before = null,
		Snowflake? after = null,
		CancellationToken cancellationToken = default)
	{
		var path = new StringBuilder("guilds/{guild_id}/scheduled-events/{event_id}/users");
		var hasQuery = false;

		void Append(string key, string value)
		{
			path.Append(hasQuery ? '&' : '?').Append(key).Append('=').Append(Uri.EscapeDataString(value));
			hasQuery = true;
		}

		if (limit is { } l) Append("limit", l.ToString());
		if (withMember is { } wm) Append("with_member", wm ? "true" : "false");
		if (before is { } b) Append("before", b.ToString());
		if (after is { } a) Append("after", a.ToString());

		var route = new DiscordRoute(path.ToString(), guildId, eventId);
		return client.SendAsync<GuildScheduledEventUser[]>(route, HttpMethod.Get, null, cancellationToken);
	}
}

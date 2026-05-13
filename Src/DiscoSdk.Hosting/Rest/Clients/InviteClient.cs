using DiscoSdk.Exceptions;
using DiscoSdk.Models;
using DiscoSdk.Rest;
using System.Text;

namespace DiscoSdk.Hosting.Rest.Clients;

/// <summary>
/// Client for Discord invite operations (create, delete, etc.).
/// </summary>
/// <param name="client">The REST client base to use for requests.</param>
internal class InviteClient(IDiscordRestClient client)
{
	/// <summary>
	/// Resolves a Discord invite by its code. Returns <c>null</c> if the code does not exist.
	/// </summary>
	/// <param name="code">The invite code to resolve.</param>
	/// <param name="withCounts">If <c>true</c>, include approximate member/presence counts on the returned invite.</param>
	/// <param name="withExpiration">If <c>true</c>, include the <c>expires_at</c> timestamp on the returned invite.</param>
	/// <param name="guildScheduledEventId">If provided, embed the matching scheduled event in the returned invite.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	public async Task<Invite?> GetAsync(string code, bool? withCounts = null, bool? withExpiration = null, Snowflake? guildScheduledEventId = null, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(code))
			throw new ArgumentException("Invite code cannot be null or empty.", nameof(code));

		var sb = new StringBuilder("invites/{code}");
		var hasQuery = false;
		void Append(string k, string v) { sb.Append(hasQuery ? '&' : '?').Append(k).Append('=').Append(v); hasQuery = true; }
		if (withCounts.HasValue) Append("with_counts", withCounts.Value ? "true" : "false");
		if (withExpiration.HasValue) Append("with_expiration", withExpiration.Value ? "true" : "false");
		if (guildScheduledEventId.HasValue) Append("guild_scheduled_event_id", guildScheduledEventId.Value.ToString());

		var route = new DiscordRoute(sb.ToString(), code);
		try
		{
			return await client.SendAsync<Invite>(route, HttpMethod.Get, null, cancellationToken);
		}
		catch (DiscordApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	/// <summary>
	/// Creates a new invite for the specified channel.
	/// </summary>
	/// <param name="channelId">The ID of the channel to create an invite for.</param>
	/// <param name="request">The invite creation request.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The created invite.</returns>
	public Task<Invite> CreateAsync(Snowflake channelId, object request, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		ArgumentNullException.ThrowIfNull(request);

		var route = new DiscordRoute("channels/{channel_id}/invites", channelId);
		return client.SendAsync<Invite>(route, HttpMethod.Post, request, cancellationToken);
	}

	/// <summary>
	/// Gets all invites for the specified channel.
	/// </summary>
	/// <param name="channelId">The ID of the channel to get invites for.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>An array of invites for the channel.</returns>
	public Task<Invite[]> GetChannelInvitesAsync(Snowflake channelId, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		var route = new DiscordRoute("channels/{channel_id}/invites", channelId);
		return client.SendAsync<Invite[]>(route, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>
	/// Deletes an invite by its code.
	/// </summary>
	/// <param name="code">The invite code to delete.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public Task DeleteAsync(string code, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(code))
			throw new ArgumentException("Invite code cannot be null or empty.", nameof(code));

		var route = new DiscordRoute("invites/{code}", code);
		return client.SendAsync(route, HttpMethod.Delete, cancellationToken);
	}
}
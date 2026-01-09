using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Rest.Clients;

/// <summary>
/// Client for Discord invite operations (create, delete, etc.).
/// </summary>
/// <param name="client">The REST client base to use for requests.</param>
internal class InviteClient(IDiscordRestClientBase client)
{
	/// <summary>
	/// Creates a new invite for the specified channel.
	/// </summary>
	/// <param name="channelId">The ID of the channel to create an invite for.</param>
	/// <param name="request">The invite creation request.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The created invite.</returns>
	public Task<Invite> CreateAsync(DiscordId channelId, object request, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		ArgumentNullException.ThrowIfNull(request);

		var path = $"channels/{channelId}/invites";
		return client.SendJsonAsync<Invite>(path, HttpMethod.Post, request, cancellationToken);
	}

	/// <summary>
	/// Gets all invites for the specified channel.
	/// </summary>
	/// <param name="channelId">The ID of the channel to get invites for.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>An array of invites for the channel.</returns>
	public Task<Invite[]> GetChannelInvitesAsync(DiscordId channelId, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		var path = $"channels/{channelId}/invites";
		return client.SendJsonAsync<Invite[]>(path, HttpMethod.Get, null, cancellationToken);
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

		var path = $"invites/{code}";
		return client.SendNoContentAsync(path, HttpMethod.Delete, cancellationToken);
	}
}
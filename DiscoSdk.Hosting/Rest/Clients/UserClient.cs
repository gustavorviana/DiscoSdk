using DiscoSdk.Models;
using DiscoSdk.Models.Users;
using DiscoSdk.Rest;

namespace DiscoSdk.Hosting.Rest.Clients;

/// <summary>
/// Client for Discord user operations (get user, etc.).
/// </summary>
/// <param name="client">The REST client base to use for requests.</param>
internal class UserClient(IDiscordRestClient client)
{
	/// <summary>
	/// Gets a user by their ID.
	/// </summary>
	/// <param name="userId">The ID of the user to retrieve.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The user, or null if not found.</returns>
	public async Task<User?> GetAsync(Snowflake userId, CancellationToken cancellationToken = default)
	{
		if (userId == default)
			throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

		var path = $"users/{userId}";
		try
		{
			return await client.SendAsync<User>(path, HttpMethod.Get, null, cancellationToken);
		}
		catch (DiscordApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			return null;
		}
	}
}


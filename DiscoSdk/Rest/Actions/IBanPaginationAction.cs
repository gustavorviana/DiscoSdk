using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a pagination action for retrieving banned members from a guild.
/// </summary>
public interface IBanPaginationAction : IPaginationAction<Ban, IBanPaginationAction>
{
	/// <summary>
	/// Gets bans before this user ID.
	/// </summary>
	/// <param name="userId">The user ID to get bans before.</param>
	/// <returns>The current <see cref="IBanPaginationAction"/> instance.</returns>
	IBanPaginationAction Before(DiscordId userId);

	/// <summary>
	/// Gets bans after this user ID.
	/// </summary>
	/// <param name="userId">The user ID to get bans after.</param>
	/// <returns>The current <see cref="IBanPaginationAction"/> instance.</returns>
	IBanPaginationAction After(DiscordId userId);
}


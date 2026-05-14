using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a Discord group direct-message channel (a DM with three or more participants).
/// </summary>
public interface IGroupDmChannel : IDmChannel
{
	/// <summary>
	/// Gets the ID of the user who owns (created) this group DM.
	/// </summary>
	Snowflake OwnerId { get; }

	/// <summary>
	/// Adds a user to this group DM. Requires an OAuth2 access token with the <c>gdm.join</c>
	/// scope from the user being added — bot tokens are rejected by Discord with <c>401</c>.
	/// </summary>
	/// <param name="userId">The ID of the user to add.</param>
	/// <param name="accessToken">The user's OAuth2 access token with the <c>gdm.join</c> scope.</param>
	/// <param name="nick">Optional nickname for the recipient inside the group DM.</param>
	IRestAction AddRecipient(Snowflake userId, string accessToken, string? nick = null);

	/// <summary>
	/// Removes a user from this group DM. The caller must own the group DM.
	/// </summary>
	/// <param name="userId">The ID of the user to remove.</param>
	IRestAction RemoveRecipient(Snowflake userId);
}

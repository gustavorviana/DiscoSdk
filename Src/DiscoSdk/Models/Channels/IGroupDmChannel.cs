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
	/// Builds a REST action that adds a user to this group DM. The OAuth2 access token (with the
	/// <c>gdm.join</c> scope) is required and is configured via <see cref="IAddGroupDmRecipientAction.SetAccessToken"/>.
	/// Bot tokens are rejected by Discord with <c>401</c>.
	/// </summary>
	IAddGroupDmRecipientAction AddRecipient(Snowflake userId);

	/// <summary>
	/// Removes a user from this group DM. The caller must own the group DM.
	/// </summary>
	/// <param name="userId">The ID of the user to remove.</param>
	IRestAction RemoveRecipient(Snowflake userId);
}

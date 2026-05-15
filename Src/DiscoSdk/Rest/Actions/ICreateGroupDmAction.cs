using DiscoSdk.Models;
using DiscoSdk.Models.Channels;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// REST action that creates a new group DM via <c>POST /users/@me/channels</c>. Each recipient
/// must have granted the application the OAuth2 <c>gdm.join</c> scope; bot tokens cannot invoke
/// the underlying endpoint at all, so the SDK assumes the caller already exchanged tokens out of
/// band.
/// </summary>
/// <remarks>
/// Discord allows between 2 and 10 recipients per group DM (the bot itself is not counted; the
/// resulting channel will include the bot plus every user identified by an access token).
/// Source: <see href="https://discord.com/developers/docs/resources/user#create-group-dm"/>.
/// </remarks>
public interface ICreateGroupDmAction : IRestAction<IGroupDmChannel>
{
	/// <summary>
	/// Adds a recipient by their OAuth2 access token (the token must carry the <c>gdm.join</c>
	/// scope). The recipient's user id is derived server-side from the token; no nickname is set.
	/// </summary>
	/// <param name="accessToken">OAuth2 access token granted by the user for the application.</param>
	ICreateGroupDmAction AddRecipient(string accessToken);

	/// <summary>
	/// Adds a recipient with a custom nickname shown inside the group DM. <paramref name="userId"/>
	/// must be the id of the user whose <paramref name="accessToken"/> was granted; Discord
	/// matches the nick entry to the access token via this user id.
	/// </summary>
	/// <param name="accessToken">OAuth2 access token granted by the user for the application.</param>
	/// <param name="userId">User id matching the access token (used as the <c>nicks</c> map key).</param>
	/// <param name="nick">Nickname to display for this user inside the group DM.</param>
	ICreateGroupDmAction AddRecipient(string accessToken, Snowflake userId, string nick);
}

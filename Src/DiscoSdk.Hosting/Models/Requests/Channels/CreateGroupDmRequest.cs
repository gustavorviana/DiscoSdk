using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Models.Requests.Channels;

/// <summary>
/// Request body for <c>POST /users/@me/channels</c> when creating a new group DM. Requires each
/// participant to have granted the application the OAuth2 <c>gdm.join</c> scope; bot tokens
/// cannot invoke this endpoint at all.
/// </summary>
/// <remarks>
/// Source: <see href="https://discord.com/developers/docs/resources/user#create-group-dm"/>.
/// </remarks>
internal sealed class CreateGroupDmRequest
{
	/// <summary>
	/// Access tokens of the users (with <c>gdm.join</c> scope) that will be added to the group DM.
	/// Discord allows between 2 and 10 entries per group DM.
	/// </summary>
	[JsonPropertyName("access_tokens")]
	public string[] AccessTokens { get; set; } = [];

	/// <summary>
	/// Optional nickname overrides keyed by user id (as a string snowflake). Each entry must
	/// correspond to a user whose access token is present in <see cref="AccessTokens"/>; otherwise
	/// Discord ignores it.
	/// </summary>
	[JsonPropertyName("nicks")]
	public Dictionary<string, string>? Nicks { get; set; }
}

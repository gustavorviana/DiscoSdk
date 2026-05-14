using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Requests.Channels;

/// <summary>
/// Request body for <c>PUT /channels/{channel.id}/recipients/{user.id}</c>.
/// Both fields require an OAuth2 access token with the <c>gdm.join</c> scope; bot tokens cannot
/// invoke this endpoint. Reference:
/// https://discord.com/developers/docs/resources/channel#group-dm-add-recipient
/// </summary>
internal class GroupDmAddRecipientRequest
{
	/// <summary>Access token of the user that granted the bot the <c>gdm.join</c> scope.</summary>
	[JsonPropertyName("access_token")]
	public string AccessToken { get; set; } = default!;

	/// <summary>Optional nickname to display for the recipient inside the group DM.</summary>
	[JsonPropertyName("nick")]
	public string? Nick { get; set; }
}

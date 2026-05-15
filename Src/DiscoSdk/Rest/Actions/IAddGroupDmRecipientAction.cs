namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Adds a user to a group DM. The OAuth2 access token (with the <c>gdm.join</c> scope) is
/// required and must come from the user being added — bot tokens are rejected.
/// </summary>
public interface IAddGroupDmRecipientAction : IRestAction
{
    /// <summary>Sets the user's OAuth2 access token (required).</summary>
    IAddGroupDmRecipientAction SetAccessToken(string accessToken);

    /// <summary>Sets a nickname for the recipient inside the group DM.</summary>
    IAddGroupDmRecipientAction SetNickname(string nick);
}

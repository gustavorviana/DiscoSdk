namespace DiscoSdk.Models.Enums;

/// <summary>
/// Optional hint for <c>POST /oauth2/token/revoke</c>'s <c>token_type_hint</c> field. Discord
/// revokes both the access and refresh tokens regardless of the hint, but supplying it helps the
/// server resolve the token faster.
/// </summary>
public enum OAuth2TokenTypeHint
{
    /// <summary>The token is an access token.</summary>
    AccessToken,

    /// <summary>The token is a refresh token.</summary>
    RefreshToken,
}

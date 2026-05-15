using DiscoSdk.Models.OAuth2;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Exchanges a temporary authorization <c>code</c> (received on the OAuth2 redirect) for an
/// access + refresh token pair. <c>client_id</c> is auto-filled from the bot. All three setters
/// are mandatory before <see cref="IRestAction{T}.ExecuteAsync"/> is called.
/// </summary>
public interface IExchangeAuthorizationCodeAction : IRestAction<IAccessTokenResponse>
{
    /// <summary>Sets the OAuth2 client secret (the auth principal).</summary>
    IExchangeAuthorizationCodeAction SetClientSecret(string clientSecret);

    /// <summary>Sets the authorization code returned to the redirect URI.</summary>
    IExchangeAuthorizationCodeAction SetCode(string code);

    /// <summary>Sets the redirect URI that was registered for the original authorize request.</summary>
    IExchangeAuthorizationCodeAction SetRedirectUri(string redirectUri);
}

using DiscoSdk.Models.Enums;
using DiscoSdk.Models.OAuth2;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk;

/// <summary>
/// Public surface for Discord's OAuth2 token-flow endpoints. Exposed via <see cref="IDiscordClient.OAuth2"/>.
/// <c>client_id</c> auto-fills from the bot's <see cref="IDiscordClient.ApplicationId"/> — callers only
/// supply the <c>client_secret</c>.
/// </summary>
public interface IOAuth2
{
    /// <summary>
    /// Fluent builder for the OAuth2 authorize URL the user opens in a browser to start the
    /// authorization-code flow. Build is synchronous — no REST call.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the client isn't ready yet (the application ID is discovered on connect).</exception>
    IBuildAuthorizeUrlAction BuildAuthorizeUrl();

    /// <summary>
    /// Builds a REST action that exchanges a temporary <c>code</c> (received on the OAuth2 redirect)
    /// for an access + refresh token pair. Configure secret / code / redirect URI on the builder.
    /// Sends HTTP Basic auth (<c>{ApplicationId}:{clientSecret}</c>).
    /// </summary>
    IExchangeAuthorizationCodeAction ExchangeAuthorizationCode();

    /// <summary>Trades a refresh token for a fresh access + refresh token pair.</summary>
    IRestAction<IAccessTokenResponse> RefreshAccessToken(string clientSecret, string refreshToken);

    /// <summary>
    /// Issues an app-scoped bearer token via the <c>client_credentials</c> grant — no user
    /// involved, no <c>refresh_token</c> in the response.
    /// </summary>
    IRestAction<IAccessTokenResponse> GetClientCredentialsToken(string clientSecret, params string[] scopes);

    /// <summary>
    /// Builds a REST action that revokes an access or refresh token. Both halves of the token
    /// pair are revoked regardless of the optional hint.
    /// </summary>
    IRevokeTokenAction RevokeToken();

    /// <summary>Reads metadata for a bearer access token: application, granted scopes, expiry, authorising user.</summary>
    IRestAction<ICurrentAuthorizationInfo> GetCurrentAuthorizationInfo(string accessToken);
}

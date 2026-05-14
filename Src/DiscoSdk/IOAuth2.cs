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
    /// Exchanges a temporary <paramref name="code"/> (received on the OAuth2 redirect) for an
    /// access + refresh token pair. Sends HTTP Basic auth (<c>{ApplicationId}:{clientSecret}</c>).
    /// </summary>
    IRestAction<IAccessTokenResponse> ExchangeAuthorizationCode(string clientSecret, string code, string redirectUri);

    /// <summary>Trades a refresh token for a fresh access + refresh token pair.</summary>
    IRestAction<IAccessTokenResponse> RefreshAccessToken(string clientSecret, string refreshToken);

    /// <summary>
    /// Issues an app-scoped bearer token via the <c>client_credentials</c> grant — no user
    /// involved, no <c>refresh_token</c> in the response.
    /// </summary>
    IRestAction<IAccessTokenResponse> GetClientCredentialsToken(string clientSecret, params string[] scopes);

    /// <summary>Revokes an access or refresh token. Both halves of the token pair are revoked regardless of <paramref name="tokenTypeHint"/>.</summary>
    IRestAction RevokeToken(string clientSecret, string token, OAuth2TokenTypeHint? tokenTypeHint = null);

    /// <summary>Reads metadata for a bearer access token: application, granted scopes, expiry, authorising user.</summary>
    IRestAction<ICurrentAuthorizationInfo> GetCurrentAuthorizationInfo(string accessToken);
}

using DiscoSdk.Models.Enums;
using DiscoSdk.Models.OAuth2;
using DiscoSdk.Rest;
using System.Net.Http.Headers;
using System.Text;

namespace DiscoSdk.Hosting.Rest.Clients;

/// <summary>
/// REST client for Discord's OAuth2 token-flow endpoints. None of these endpoints use the bot's
/// <c>Authorization</c> default: <see cref="ExchangeAuthorizationCodeAsync"/>,
/// <see cref="RefreshAccessTokenAsync"/>, <see cref="GetClientCredentialsTokenAsync"/>, and
/// <see cref="RevokeTokenAsync"/> send HTTP Basic auth derived from
/// <c>client_id:client_secret</c>; <see cref="GetCurrentAuthorizationInfoAsync"/> sends a Bearer
/// access token. The bot token on the shared <c>HttpClient.DefaultRequestHeaders</c> is never
/// mutated — overrides are per-<see cref="HttpRequestMessage"/>.
/// </summary>
internal class OAuth2Client(IDiscordRestClient client)
{
    /// <summary>
    /// Exchanges a temporary authorization code (from the redirect-back step) for a real
    /// access + refresh token pair.
    /// Reference: https://discord.com/developers/docs/topics/oauth2#authorization-code-grant
    /// </summary>
    public Task<AccessTokenResponse> ExchangeAuthorizationCodeAsync(
        string clientId,
        string clientSecret,
        string code,
        string redirectUri,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientSecret);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(redirectUri);

        var fields = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = redirectUri,
        };

        return client.SendFormUrlEncodedAsync<AccessTokenResponse>(
            new DiscordRoute("oauth2/token"),
            HttpMethod.Post,
            fields,
            BasicAuth(clientId, clientSecret),
            cancellationToken);
    }

    /// <summary>
    /// Trades a long-lived refresh token for a fresh access token (and a new refresh token).
    /// Use when an access token is close to expiry.
    /// </summary>
    public Task<AccessTokenResponse> RefreshAccessTokenAsync(
        string clientId,
        string clientSecret,
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientSecret);
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        var fields = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
        };

        return client.SendFormUrlEncodedAsync<AccessTokenResponse>(
            new DiscordRoute("oauth2/token"),
            HttpMethod.Post,
            fields,
            BasicAuth(clientId, clientSecret),
            cancellationToken);
    }

    /// <summary>
    /// Issues an app-scoped access token via the <c>client_credentials</c> grant — no user
    /// involved. Unlike the user grants, this response has no <c>refresh_token</c>; call again
    /// when the token expires. Reference:
    /// https://discord.com/developers/docs/topics/oauth2#client-credentials-grant
    /// </summary>
    public Task<AccessTokenResponse> GetClientCredentialsTokenAsync(
        string clientId,
        string clientSecret,
        IEnumerable<string> scopes,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientSecret);
        ArgumentNullException.ThrowIfNull(scopes);

        var joined = string.Join(' ', scopes);
        if (string.IsNullOrWhiteSpace(joined))
            throw new ArgumentException("At least one scope is required.", nameof(scopes));

        var fields = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["scope"] = joined,
        };

        return client.SendFormUrlEncodedAsync<AccessTokenResponse>(
            new DiscordRoute("oauth2/token"),
            HttpMethod.Post,
            fields,
            BasicAuth(clientId, clientSecret),
            cancellationToken);
    }

    /// <summary>
    /// Revokes an access or refresh token. Discord revokes both halves of the token pair
    /// regardless of <paramref name="tokenTypeHint"/>; the hint just speeds up resolution.
    /// </summary>
    public Task RevokeTokenAsync(
        string clientId,
        string clientSecret,
        string token,
        OAuth2TokenTypeHint? tokenTypeHint = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientSecret);
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var fields = new Dictionary<string, string>
        {
            ["token"] = token,
        };
        if (tokenTypeHint is { } hint)
            fields["token_type_hint"] = hint == OAuth2TokenTypeHint.AccessToken ? "access_token" : "refresh_token";

        return client.SendFormUrlEncodedAsync(
            new DiscordRoute("oauth2/token/revoke"),
            HttpMethod.Post,
            fields,
            BasicAuth(clientId, clientSecret),
            cancellationToken);
    }

    /// <summary>
    /// Reads the metadata for a bearer access token: which application, granted scopes, expiry,
    /// and authorising user (only when <c>identify</c> was granted).
    /// </summary>
    public Task<CurrentAuthorizationInfo> GetCurrentAuthorizationInfoAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);

        return client.SendAsync<CurrentAuthorizationInfo>(
            new DiscordRoute("oauth2/@me"),
            HttpMethod.Get,
            null,
            new AuthenticationHeaderValue("Bearer", accessToken),
            cancellationToken);
    }

    private static AuthenticationHeaderValue BasicAuth(string clientId, string clientSecret)
    {
        var raw = $"{clientId}:{clientSecret}";
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
        return new AuthenticationHeaderValue("Basic", encoded);
    }
}

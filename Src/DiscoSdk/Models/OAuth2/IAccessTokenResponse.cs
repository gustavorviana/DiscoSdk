namespace DiscoSdk.Models.OAuth2;

/// <summary>
/// Public read-only view of an OAuth2 token response (authorization-code, refresh-token, or
/// client-credentials grant). Callers persist <see cref="RefreshToken"/> server-side to renew
/// <see cref="AccessToken"/> when it expires.
/// </summary>
public interface IAccessTokenResponse
{
    /// <summary>The bearer token used for subsequent API calls on behalf of the resource owner.</summary>
    string AccessToken { get; }

    /// <summary>Token type — always <c>"Bearer"</c> for Discord's OAuth2 endpoints.</summary>
    string TokenType { get; }

    /// <summary>Lifetime of <see cref="AccessToken"/> in seconds (Discord defaults to 7 days).</summary>
    int ExpiresIn { get; }

    /// <summary>
    /// Long-lived token used to obtain a new access token without re-prompting the user. Null for
    /// the <c>client_credentials</c> grant (Discord doesn't issue refresh tokens for app-scoped
    /// tokens).
    /// </summary>
    string? RefreshToken { get; }

    /// <summary>Space-separated scope list as Discord returns it (e.g. <c>"identify guilds"</c>).</summary>
    string Scope { get; }
}

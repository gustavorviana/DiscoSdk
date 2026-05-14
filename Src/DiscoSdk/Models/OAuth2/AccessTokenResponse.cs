using System.Text.Json.Serialization;

namespace DiscoSdk.Models.OAuth2;

/// <summary>
/// Wire-format response from <c>POST /oauth2/token</c> (all three grant types). RFC 6749 §5.1.
/// Reference: https://discord.com/developers/docs/topics/oauth2#authorization-code-grant
/// </summary>
internal class AccessTokenResponse : IAccessTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = default!;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "Bearer";

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Null for the <c>client_credentials</c> grant — Discord doesn't issue refresh tokens for
    /// app-scoped tokens. Non-null for <c>authorization_code</c> and <c>refresh_token</c>.
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    /// <summary>Space-separated scope list as Discord serialises it (e.g. <c>"identify guilds"</c>).</summary>
    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;
}

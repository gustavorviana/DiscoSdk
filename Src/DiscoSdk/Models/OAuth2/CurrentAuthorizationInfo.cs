using DiscoSdk.Models.Applications;
using DiscoSdk.Models.Users;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.OAuth2;

/// <summary>
/// Wire-format response from <c>GET /oauth2/@me</c>. Reveals which application the bearer token
/// belongs to, which scopes were granted, when it expires, and (when <c>identify</c> is in scope)
/// which user authorised it.
/// Reference: https://discord.com/developers/docs/topics/oauth2#get-current-authorization-information
/// </summary>
internal class CurrentAuthorizationInfo
{
    [JsonPropertyName("application")]
    [JsonInclude]
    internal Application Application { get; set; } = default!;

    /// <summary>Granted scopes (already split, unlike <see cref="IAccessTokenResponse.Scope"/>).</summary>
    [JsonPropertyName("scopes")]
    public string[] Scopes { get; set; } = [];

    [JsonPropertyName("expires")]
    public DateTimeOffset Expires { get; set; }

    /// <summary>User who authorised — null if <c>identify</c> wasn't granted.</summary>
    [JsonPropertyName("user")]
    public User? User { get; set; }
}

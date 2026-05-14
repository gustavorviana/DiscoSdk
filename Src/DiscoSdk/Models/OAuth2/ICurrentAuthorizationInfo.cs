using DiscoSdk.Models.Applications;

namespace DiscoSdk.Models.OAuth2;

/// <summary>
/// Public read-only view of <c>GET /oauth2/@me</c>. Useful for verifying which scopes a stored
/// bearer token still has and when it expires before deciding to refresh.
/// </summary>
public interface ICurrentAuthorizationInfo
{
    /// <summary>The application the bearer token belongs to.</summary>
    IApplication Application { get; }

    /// <summary>Granted scopes (split, e.g. <c>["identify", "guilds"]</c>).</summary>
    IReadOnlyList<string> Scopes { get; }

    /// <summary>When the access token expires.</summary>
    DateTimeOffset Expires { get; }

    /// <summary>
    /// The user that authorised — only populated when <c>identify</c> is in <see cref="Scopes"/>.
    /// Null for the <c>client_credentials</c> grant.
    /// </summary>
    IUser? User { get; }
}

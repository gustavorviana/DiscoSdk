using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Revokes an access or refresh token. Discord always revokes both halves of the token pair
/// regardless of the optional hint. <see cref="SetClientSecret"/> and <see cref="SetToken"/>
/// are mandatory before <see cref="IRestAction.ExecuteAsync"/>.
/// </summary>
public interface IRevokeTokenAction : IRestAction
{
    /// <summary>Sets the OAuth2 client secret (the auth principal).</summary>
    IRevokeTokenAction SetClientSecret(string clientSecret);

    /// <summary>Sets the token to revoke (access or refresh).</summary>
    IRevokeTokenAction SetToken(string token);

    /// <summary>Hints to Discord which kind of token this is — performance only.</summary>
    IRevokeTokenAction WithHint(OAuth2TokenTypeHint hint);
}

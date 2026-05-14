namespace DiscoSdk.Models.Enums;

/// <summary>
/// Value of the <c>response_type</c> query parameter on the authorize URL.
/// </summary>
public enum OAuth2ResponseType
{
    /// <summary>
    /// Authorization Code grant — Discord redirects back with a <c>?code=...</c> that the server
    /// then exchanges for tokens. Almost always what you want.
    /// </summary>
    Code,

    /// <summary>
    /// Implicit grant — Discord redirects back with the access token in the URL fragment.
    /// Browser-only flow with no refresh-token support; avoid for new code.
    /// </summary>
    Token,
}

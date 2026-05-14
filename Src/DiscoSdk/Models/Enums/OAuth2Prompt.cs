namespace DiscoSdk.Models.Enums;

/// <summary>
/// Value of the <c>prompt</c> query parameter on the authorize URL — controls whether Discord
/// shows the authorization dialog again when the user has previously authorized the app.
/// </summary>
public enum OAuth2Prompt
{
    /// <summary>Force the consent screen to appear even if the user previously authorized.</summary>
    Consent,

    /// <summary>Skip the consent screen when the user has already authorized the requested scopes.</summary>
    None,
}

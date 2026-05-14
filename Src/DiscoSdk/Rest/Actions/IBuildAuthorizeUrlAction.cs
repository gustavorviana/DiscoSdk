using DiscoSdk.Models;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Fluent builder for Discord's OAuth2 authorize URL — the URL the user opens in their browser
/// to start the authorization flow. Synchronous; <see cref="Build"/> never hits the network. The
/// builder isn't an <see cref="IRestAction{T}"/> because there's no REST call to defer.
/// Reference: https://discord.com/developers/docs/topics/oauth2#authorization-code-grant-authorization-url-example
/// </summary>
public interface IBuildAuthorizeUrlAction
{
    /// <summary>Sets <c>response_type</c>. Defaults to <see cref="OAuth2ResponseType.Code"/>.</summary>
    IBuildAuthorizeUrlAction SetResponseType(OAuth2ResponseType responseType);

    /// <summary>
    /// Sets the requested scopes. Multiple calls replace the previous set. Pass scope constants
    /// from <see cref="DiscoSdk.Models.OAuth2.OAuth2Scope"/>.
    /// </summary>
    IBuildAuthorizeUrlAction SetScopes(params string[] scopes);

    /// <summary>Sets <c>redirect_uri</c> — must match a redirect URI registered for the app.</summary>
    IBuildAuthorizeUrlAction SetRedirectUri(string redirectUri);

    /// <summary>Sets <c>state</c> — anti-CSRF token echoed back on the redirect.</summary>
    IBuildAuthorizeUrlAction SetState(string state);

    /// <summary>Sets <c>prompt</c> — controls re-display of the consent screen.</summary>
    IBuildAuthorizeUrlAction SetPrompt(OAuth2Prompt prompt);

    /// <summary>Sets <c>permissions</c> — used when requesting the <c>bot</c> scope to pre-select the bot's permissions integer.</summary>
    IBuildAuthorizeUrlAction SetPermissions(ulong permissions);

    /// <summary>Sets <c>guild_id</c> — pre-selects a guild when installing the bot.</summary>
    IBuildAuthorizeUrlAction SetGuildId(Snowflake guildId);

    /// <summary>Sets <c>disable_guild_select</c> — locks the guild picker to <see cref="SetGuildId"/>.</summary>
    IBuildAuthorizeUrlAction SetDisableGuildSelect(bool disable);

    /// <summary>Constructs the final URL. Safe to call multiple times.</summary>
    string Build();
}

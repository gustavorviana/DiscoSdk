using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;
using System.Text;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Builds an OAuth2 authorize URL. Pure string composition — no network, no DiscordClient
/// dependency beyond receiving <paramref name="clientId"/>.
/// </summary>
internal sealed class BuildAuthorizeUrlAction(string clientId) : IBuildAuthorizeUrlAction
{
    private const string AuthorizeEndpoint = "https://discord.com/oauth2/authorize";

    private readonly string _clientId = !string.IsNullOrWhiteSpace(clientId)
        ? clientId
        : throw new ArgumentException("Client ID is required.", nameof(clientId));

    private OAuth2ResponseType _responseType = OAuth2ResponseType.Code;
    private string[]? _scopes;
    private string? _redirectUri;
    private string? _state;
    private OAuth2Prompt? _prompt;
    private ulong? _permissions;
    private Snowflake? _guildId;
    private bool? _disableGuildSelect;

    public IBuildAuthorizeUrlAction SetResponseType(OAuth2ResponseType responseType)
    {
        _responseType = responseType;
        return this;
    }

    public IBuildAuthorizeUrlAction SetScopes(params string[] scopes)
    {
        ArgumentNullException.ThrowIfNull(scopes);
        if (scopes.Length == 0)
            throw new ArgumentException("At least one scope is required.", nameof(scopes));

        _scopes = scopes;
        return this;
    }

    public IBuildAuthorizeUrlAction SetRedirectUri(string redirectUri)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(redirectUri);
        _redirectUri = redirectUri;
        return this;
    }

    public IBuildAuthorizeUrlAction SetState(string state)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(state);
        _state = state;
        return this;
    }

    public IBuildAuthorizeUrlAction SetPrompt(OAuth2Prompt prompt)
    {
        _prompt = prompt;
        return this;
    }

    public IBuildAuthorizeUrlAction SetPermissions(ulong permissions)
    {
        _permissions = permissions;
        return this;
    }

    public IBuildAuthorizeUrlAction SetGuildId(Snowflake guildId)
    {
        _guildId = guildId;
        return this;
    }

    public IBuildAuthorizeUrlAction SetDisableGuildSelect(bool disable)
    {
        _disableGuildSelect = disable;
        return this;
    }

    public string Build()
    {
        if (_scopes is null || _scopes.Length == 0)
            throw new InvalidOperationException("At least one scope must be set before building the URL.");

        var sb = new StringBuilder(AuthorizeEndpoint);
        sb.Append('?').Append("client_id=").Append(Uri.EscapeDataString(_clientId));
        sb.Append('&').Append("response_type=").Append(_responseType == OAuth2ResponseType.Code ? "code" : "token");
        sb.Append('&').Append("scope=").Append(Uri.EscapeDataString(string.Join(' ', _scopes)));

        if (_redirectUri is not null)
            sb.Append('&').Append("redirect_uri=").Append(Uri.EscapeDataString(_redirectUri));
        if (_state is not null)
            sb.Append('&').Append("state=").Append(Uri.EscapeDataString(_state));
        if (_prompt is { } p)
            sb.Append('&').Append("prompt=").Append(p == OAuth2Prompt.Consent ? "consent" : "none");
        if (_permissions is { } perms)
            sb.Append('&').Append("permissions=").Append(perms.ToString(System.Globalization.CultureInfo.InvariantCulture));
        if (_guildId is { } gid)
            sb.Append('&').Append("guild_id=").Append(gid.ToString());
        if (_disableGuildSelect is { } dgs)
            sb.Append('&').Append("disable_guild_select=").Append(dgs ? "true" : "false");

        return sb.ToString();
    }
}

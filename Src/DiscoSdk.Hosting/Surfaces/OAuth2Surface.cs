using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.OAuth2;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Surfaces;

/// <summary>
/// Concrete implementation of <see cref="IOAuth2"/>. Thin facade that auto-fills <c>client_id</c>
/// from the bot's <see cref="DiscordClient.ApplicationId"/> and delegates to <see cref="OAuth2Client"/>.
/// </summary>
internal sealed class OAuth2Surface(DiscordClient client) : IOAuth2
{
    private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));

    /// <inheritdoc />
    public IBuildAuthorizeUrlAction BuildAuthorizeUrl()
        => new BuildAuthorizeUrlAction(_client.RequireApplicationId().ToString());

    /// <inheritdoc />
    public IRestAction<IAccessTokenResponse> ExchangeAuthorizationCode(string clientSecret, string code, string redirectUri)
        => RestAction<IAccessTokenResponse>.Create(async ct =>
            await _client.OAuth2Client.ExchangeAuthorizationCodeAsync(_client.RequireApplicationId().ToString(), clientSecret, code, redirectUri, ct));

    /// <inheritdoc />
    public IRestAction<IAccessTokenResponse> RefreshAccessToken(string clientSecret, string refreshToken)
        => RestAction<IAccessTokenResponse>.Create(async ct =>
            await _client.OAuth2Client.RefreshAccessTokenAsync(_client.RequireApplicationId().ToString(), clientSecret, refreshToken, ct));

    /// <inheritdoc />
    public IRestAction<IAccessTokenResponse> GetClientCredentialsToken(string clientSecret, params string[] scopes)
        => RestAction<IAccessTokenResponse>.Create(async ct =>
            await _client.OAuth2Client.GetClientCredentialsTokenAsync(_client.RequireApplicationId().ToString(), clientSecret, scopes, ct));

    /// <inheritdoc />
    public IRestAction RevokeToken(string clientSecret, string token, OAuth2TokenTypeHint? tokenTypeHint = null)
        => RestAction.Create(ct => _client.OAuth2Client.RevokeTokenAsync(_client.RequireApplicationId().ToString(), clientSecret, token, tokenTypeHint, ct));

    /// <inheritdoc />
    public IRestAction<ICurrentAuthorizationInfo> GetCurrentAuthorizationInfo(string accessToken)
        => RestAction<ICurrentAuthorizationInfo>.Create(async ct =>
        {
            var info = await _client.OAuth2Client.GetCurrentAuthorizationInfoAsync(accessToken, ct);
            return new CurrentAuthorizationInfoWrapper(_client, info);
        });
}

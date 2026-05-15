using DiscoSdk.Models.OAuth2;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

internal sealed class ExchangeAuthorizationCodeAction : RestAction<IAccessTokenResponse>, IExchangeAuthorizationCodeAction
{
    private readonly DiscordClient _client;
    private string? _clientSecret;
    private string? _code;
    private string? _redirectUri;

    public ExchangeAuthorizationCodeAction(DiscordClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public IExchangeAuthorizationCodeAction SetClientSecret(string clientSecret)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientSecret);
        _clientSecret = clientSecret;
        return this;
    }

    public IExchangeAuthorizationCodeAction SetCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        _code = code;
        return this;
    }

    public IExchangeAuthorizationCodeAction SetRedirectUri(string redirectUri)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(redirectUri);
        _redirectUri = redirectUri;
        return this;
    }

    public override async Task<IAccessTokenResponse> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (_clientSecret is null) throw new InvalidOperationException("Client secret is required. Call SetClientSecret(...).");
        if (_code is null) throw new InvalidOperationException("Code is required. Call SetCode(...).");
        if (_redirectUri is null) throw new InvalidOperationException("Redirect URI is required. Call SetRedirectUri(...).");

        return await _client.OAuth2Client.ExchangeAuthorizationCodeAsync(
            _client.RequireApplicationId().ToString(), _clientSecret, _code, _redirectUri, cancellationToken);
    }
}

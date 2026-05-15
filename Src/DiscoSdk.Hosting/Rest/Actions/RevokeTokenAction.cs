using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

internal sealed class RevokeTokenAction : RestAction, IRevokeTokenAction
{
    private readonly DiscordClient _client;
    private string? _clientSecret;
    private string? _token;
    private OAuth2TokenTypeHint? _hint;

    public RevokeTokenAction(DiscordClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public IRevokeTokenAction SetClientSecret(string clientSecret)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientSecret);
        _clientSecret = clientSecret;
        return this;
    }

    public IRevokeTokenAction SetToken(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        _token = token;
        return this;
    }

    public IRevokeTokenAction WithHint(OAuth2TokenTypeHint hint)
    {
        _hint = hint;
        return this;
    }

    public override Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (_clientSecret is null) throw new InvalidOperationException("Client secret is required. Call SetClientSecret(...).");
        if (_token is null) throw new InvalidOperationException("Token is required. Call SetToken(...).");

        return _client.OAuth2Client.RevokeTokenAsync(_client.RequireApplicationId().ToString(), _clientSecret, _token, _hint, cancellationToken);
    }
}

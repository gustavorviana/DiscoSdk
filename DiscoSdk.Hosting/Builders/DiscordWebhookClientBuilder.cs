using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Hosting.Rest.Clients;
using System.Text.Json;

namespace DiscoSdk.Hosting.Builders;

public sealed class DiscordWebhookClientBuilder
{
    private string? _webhookUrl;
    private string? _webhookId;
    private string? _webhookToken;

    private TimeSpan? _timeout;

    private JsonSerializerOptions? _jsonOptions;

    public DiscordWebhookClientBuilder WithWebhookUrl(string webhookUrl)
    {
        _webhookUrl = webhookUrl;
        _webhookId = null;
        _webhookToken = null;
        return this;
    }

    public DiscordWebhookClientBuilder WithWebhook(string webhookId, string webhookToken)
    {
        _webhookId = webhookId;
        _webhookToken = webhookToken;
        _webhookUrl = null;
        return this;
    }

    public DiscordWebhookClientBuilder WithTimeout(TimeSpan timeout)
    {
        _timeout = timeout;
        return this;
    }

    public DiscordWebhookClientBuilder WithJsonOptions(JsonSerializerOptions options)
    {
        _jsonOptions = options;
        return this;
    }

    public async Task<IDiscordWebhookClient> BuildAsync(CancellationToken cancellationToken = default)
    {
        var url = ResolveWebhookUrl();
        var identity = WebhookIdentity.FromUrl(url)
            ?? throw new InvalidOperationException("Invalid Discord webhook url. Expected: https://discord.com/api/webhooks/{id}/{token}");
        var jsonOptions = _jsonOptions ?? new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var client = new WebhookMessageClient(new DiscordRestClient(new Uri("https://discord.com/api/"), jsonOptions, _timeout));
        var info = await client.GetInfoAsync(identity!, cancellationToken);

        return new DiscordWebhookClient(client, info, identity);
    }

    private string ResolveWebhookUrl()
    {
        if (!string.IsNullOrWhiteSpace(_webhookUrl))
            return _webhookUrl!;

        if (string.IsNullOrWhiteSpace(_webhookId) || string.IsNullOrWhiteSpace(_webhookToken))
            throw new InvalidOperationException("You must set WithWebhookUrl(url) or WithWebhook(id, token).");

        return $"https://discord.com/api/webhooks/{_webhookId}/{_webhookToken}";
    }
}
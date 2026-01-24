using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Hosting.Logging;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Models.JsonConverters;
using System.Text.Json;

namespace DiscoSdk.Hosting.Builders;

public sealed class DiscordWebhookClientBuilder
{
    private readonly WebhookIdentity _identity;

    private TimeSpan? _timeout;

    private JsonSerializerOptions? _jsonOptions;

    public DiscordWebhookClientBuilder(string webhookUrl)
    {
        _identity = WebhookIdentity.FromUrl(webhookUrl)
            ?? throw new InvalidOperationException("Invalid Discord webhook url. Expected: https://discord.com/api/webhooks/{id}/{token}");
    }

    public DiscordWebhookClientBuilder(Snowflake id, string token)
    {
        _identity = new WebhookIdentity(id, token);
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
        var jsonOptions = _jsonOptions ?? DiscoJson.Create();
        var client = new WebhookMessageClient(new DiscordRestClient(new Uri("https://discord.com/api/"), jsonOptions, new ConsoleLogger(), _timeout));
        var info = await client.GetInfoAsync(_identity, cancellationToken);

        return new DiscordWebhookClient(client, info, _identity);
    }
}
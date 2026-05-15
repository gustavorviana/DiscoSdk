using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

internal sealed class CreateWebhookAction : RestAction<IWebhook>, ICreateWebhookAction
{
    private readonly DiscordClient _client;
    private readonly Snowflake _channelId;
    private string? _name;
    private string? _avatar;

    public CreateWebhookAction(DiscordClient client, Snowflake channelId)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _channelId = channelId;
    }

    public ICreateWebhookAction SetName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (name.Length is < 1 or > 80)
            throw new ArgumentOutOfRangeException(nameof(name), "Webhook name must be 1–80 characters.");
        _name = name;
        return this;
    }

    public ICreateWebhookAction SetAvatar(string avatarDataUri)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(avatarDataUri);
        _avatar = avatarDataUri;
        return this;
    }

    public override async Task<IWebhook> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (_name is null)
            throw new InvalidOperationException("Webhook name is required. Call SetName(...) before executing.");
        var webhook = await _client.WebhookClient.CreateAsync(_channelId, _name, _avatar, cancellationToken);
        return new WebhookWrapper(_client, webhook);
    }
}

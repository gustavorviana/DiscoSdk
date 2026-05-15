using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

internal sealed class ModifyWebhookAction : RestAction<IWebhook>, IModifyWebhookAction
{
    private readonly DiscordClient _client;
    private readonly Snowflake _webhookId;
    private string? _name;
    private string? _avatar;
    private bool _setAvatar;
    private Snowflake? _channelId;

    public ModifyWebhookAction(DiscordClient client, Snowflake webhookId)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _webhookId = webhookId;
    }

    public IModifyWebhookAction SetName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _name = name;
        return this;
    }

    public IModifyWebhookAction SetAvatar(string avatarDataUri)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(avatarDataUri);
        _avatar = avatarDataUri;
        _setAvatar = true;
        return this;
    }

    public IModifyWebhookAction ClearAvatar()
    {
        _avatar = null;
        _setAvatar = true;
        return this;
    }

    public IModifyWebhookAction MoveToChannel(Snowflake channelId)
    {
        _channelId = channelId;
        return this;
    }

    public override async Task<IWebhook> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?>();
        if (_name is not null) body["name"] = _name;
        if (_setAvatar) body["avatar"] = _avatar;
        if (_channelId.HasValue) body["channel_id"] = _channelId.Value.ToString();

        var updated = await _client.WebhookClient.ModifyAsync(_webhookId, body, cancellationToken);
        return new WebhookWrapper(_client, updated);
    }
}

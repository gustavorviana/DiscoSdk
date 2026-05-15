using DiscoSdk.Models;
using DiscoSdk.Models.Requests.Channels;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

internal sealed class AddGroupDmRecipientAction : RestAction, IAddGroupDmRecipientAction
{
    private readonly DiscordClient _client;
    private readonly Snowflake _channelId;
    private readonly Snowflake _userId;
    private string? _accessToken;
    private string? _nick;

    public AddGroupDmRecipientAction(DiscordClient client, Snowflake channelId, Snowflake userId)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _channelId = channelId;
        _userId = userId;
    }

    public IAddGroupDmRecipientAction SetAccessToken(string accessToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);
        _accessToken = accessToken;
        return this;
    }

    public IAddGroupDmRecipientAction SetNickname(string nick)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nick);
        _nick = nick;
        return this;
    }

    public override Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (_accessToken is null)
            throw new InvalidOperationException("Access token is required. Call SetAccessToken(...).");
        var request = new GroupDmAddRecipientRequest { AccessToken = _accessToken, Nick = _nick };
        return _client.ChannelClient.AddGroupDmRecipientAsync(_channelId, _userId, request, cancellationToken);
    }
}

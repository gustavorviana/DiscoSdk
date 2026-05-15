using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

internal sealed class GetInviteAction : RestAction<IInvite?>, IGetInviteAction
{
    private readonly DiscordClient _client;
    private readonly string _code;
    private bool? _withCounts;
    private bool? _withExpiration;
    private Snowflake? _guildScheduledEventId;

    public GetInviteAction(DiscordClient client, string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _code = code;
    }

    public IGetInviteAction WithCounts() { _withCounts = true; return this; }
    public IGetInviteAction WithExpiration() { _withExpiration = true; return this; }
    public IGetInviteAction WithScheduledEvent(Snowflake guildScheduledEventId)
    {
        _guildScheduledEventId = guildScheduledEventId;
        return this;
    }

    public override async Task<IInvite?> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var invite = await _client.InviteClient.GetAsync(_code, _withCounts, _withExpiration, _guildScheduledEventId, cancellationToken);
        if (invite is null) return null;

        if (invite.Channel?.Id is { } chId)
        {
            var channel = await _client.GetChannel(chId).ExecuteAsync(cancellationToken);
            if (channel is IGuildChannelBase guildChannel)
                return new InviteWrapper(invite, guildChannel, _client);
        }
        return null;
    }
}

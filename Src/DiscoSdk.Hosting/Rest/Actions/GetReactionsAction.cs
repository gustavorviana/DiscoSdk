using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

internal sealed class GetReactionsAction : RestAction<IReadOnlyList<IUser>>, IGetReactionsAction
{
    private readonly DiscordClient _client;
    private readonly Snowflake _channelId;
    private readonly Snowflake _messageId;
    private readonly string _emoji;
    private string? _after;
    private int? _limit;

    public GetReactionsAction(DiscordClient client, Snowflake channelId, Snowflake messageId, string emoji)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(emoji);
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _channelId = channelId;
        _messageId = messageId;
        _emoji = emoji;
    }

    public IGetReactionsAction After(Snowflake userId)
    {
        _after = userId.ToString();
        return this;
    }

    public IGetReactionsAction SetLimit(int limit)
    {
        if (limit is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 100.");
        _limit = limit;
        return this;
    }

    public override async Task<IReadOnlyList<IUser>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var users = await _client.MessageClient.GetReactionsAsync(_channelId, _messageId, _emoji, _after, _limit, cancellationToken);
        return users.Select(u => (IUser)new UserWrapper(_client, u)).ToList().AsReadOnly();
    }
}

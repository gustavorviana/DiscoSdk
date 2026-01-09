using DiscoSdk.Hosting.Wrappers.Channels;
using DiscoSdk.Models.Channels;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="IThreadChannelPaginationAction"/> for retrieving thread channels.
/// </summary>
internal class ThreadChannelPaginationAction : RestAction<IGuildThreadChannel[]>, IThreadChannelPaginationAction
{
    private readonly DiscordClient _client;
    private readonly IGuildChannel _channel;
    private readonly bool _private;
    private int? _limit;
    private string? _before;
    private bool _archived;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThreadChannelPaginationAction"/> class.
    /// </summary>
    /// <param name="client">The Discord client.</param>
    /// <param name="channel">The channel to get threads from.</param>
    /// <param name="archived">Whether to get archived threads.</param>
    /// <param name="isPrivate">Whether to get private archived threads (only if archived is true).</param>
    public ThreadChannelPaginationAction(DiscordClient client, IGuildChannel channel, bool archived = false, bool isPrivate = false)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _channel = channel;
        _archived = archived;
        _private = isPrivate;
    }

    public IThreadChannelPaginationAction Limit(int limit)
    {
        if (limit < 1 || limit > 100)
            throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 100.");

        _limit = limit;
        return this;
    }

    public IThreadChannelPaginationAction Before(string timestamp)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(timestamp);
        _before = timestamp;
        return this;
    }

    public override async Task<IGuildThreadChannel[]> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        Channel[] threads;
        if (_archived)
        {
            if (_private)
                threads = await _client.ChannelClient.GetPrivateArchivedThreadsAsync(_channel.Id, _before, _limit, cancellationToken);
            else
                threads = await _client.ChannelClient.GetPublicArchivedThreadsAsync(_channel.Id, _before, _limit, cancellationToken);
        }
        else
        {
            threads = await _client.ChannelClient.GetActiveThreadsAsync(_channel.Id, cancellationToken);
        }

        return [.. threads
            .Select(ch => new GuildThreadChannelWrapper(ch, _channel.Guild, _client))
            .Cast<IGuildThreadChannel>()];
    }
}
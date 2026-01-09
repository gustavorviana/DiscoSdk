using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers.Channels;

/// <summary>
/// Wrapper that implements <see cref="IGuildThreadChannel"/> for a <see cref="Channel"/> instance.
/// </summary>
internal class GuildThreadChannelWrapper : TextBasedChannelWrapper, IGuildThreadChannel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GuildThreadChannelWrapper"/> class.
    /// </summary>
    /// <param name="channel">The channel instance to wrap.</param>
    /// <param name="client">The Discord client for performing operations.</param>
    public GuildThreadChannelWrapper(Channel channel, IGuild guild, DiscordClient client)
        : base(channel, client)
    {
        _guild = guild;
    }

    private readonly IGuild _guild;

    public int? MessageCount => _channel.MessageCount;

    /// <inheritdoc />
    public int? MemberCount => _channel.MemberCount;
    public int? TotalMessageSent => _channel.TotalMessageSent;
    public ThreadAutoArchiveDuration? AutoArchiveDuration => _channel.ThreadMetadata?.AutoArchiveDuration;
    public DiscordId? OwnerId => _channel.OwnerId;
    public ThreadMetadata? Metadata => _channel.ThreadMetadata;
    public DiscordId[]? AppliedTags => _channel.AppliedTags;

    public IRestAction<IGuildThreadChannel> JoinThread()
    {
        return RestAction<IGuildThreadChannel>.Create(async cancellationToken =>
        {
            await _client.ChannelClient.JoinThreadAsync(Id, cancellationToken);
            return this;
        });
    }

    public IRestAction<IGuildThreadChannel> LeaveThread()
    {
        return RestAction<IGuildThreadChannel>.Create(async cancellationToken =>
        {
            await _client.ChannelClient.LeaveThreadAsync(Id, cancellationToken);
            return this;
        });
    }

    public IRestAction<IGuildThreadChannel> AddThreadMember(DiscordId userId)
    {
        if (userId == default)
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

        return RestAction<IGuildThreadChannel>.Create(async cancellationToken =>
        {
            await _client.ChannelClient.AddThreadMemberAsync(_channel.Id, userId, cancellationToken);
            return this;
        });
    }

    public IRestAction<IGuildThreadChannel> RemoveThreadMember(DiscordId userId)
    {
        if (userId == default)
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

        return RestAction<IGuildThreadChannel>.Create(async cancellationToken =>
        {
            await _client.ChannelClient.RemoveThreadMemberAsync(Id, userId, cancellationToken);
            return this;
        });
    }

    public IRestAction<IGuildThreadChannel> ArchiveThread()
    {
        return RestAction<IGuildThreadChannel>.Create(async cancellationToken =>
        {
            await _client.ChannelClient.ArchiveThreadAsync(Id, cancellationToken);
            return this;
        });
    }

    public IRestAction<IGuildThreadChannel> UnarchiveThread()
    {
        return RestAction<IGuildThreadChannel>.Create(async cancellationToken =>
        {
            await _client.ChannelClient.UnarchiveThreadAsync(Id, cancellationToken);
            return this;
        });
    }

    public IRestAction<IGuildThreadChannel> LockThread()
    {
        return RestAction<IGuildThreadChannel>.Create(async cancellationToken =>
        {
            await _client.ChannelClient.LockThreadAsync(Id, cancellationToken);
            return this;
        });
    }

    public IRestAction<IGuildThreadChannel> UnlockThread()
    {
        return RestAction<IGuildThreadChannel>.Create(async cancellationToken =>
        {
            await _client.ChannelClient.UnlockThreadAsync(Id, cancellationToken);
            return this;
        });
    }

    public IRestAction<IChannel?> GetParentChannelId()
    {
        return RestAction<IChannel?>.Create(async cancellationToken =>
        {
            if (_channel.ParentId is null)
                return null;

            var parent = await _client.ChannelClient.GetAsync(_channel.ParentId.Value, cancellationToken);
            return new ChannelUnionWrapper(parent, _guild, _client);
        });
    }
}
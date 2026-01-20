using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Utils;
using DiscoSdk.Hosting.Wrappers.Managers;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;
using System;

namespace DiscoSdk.Hosting.Wrappers.Channels;

/// <summary>
/// Wrapper that implements <see cref="IGuildThreadChannel"/> for a <see cref="Channel"/> instance.
/// </summary>
internal class GuildThreadChannelWrapper : TextBasedChannelWrapper, IGuildThreadChannel
{
    public int Position => _channel.Position ?? 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="GuildThreadChannelWrapper"/> class.
    /// </summary>
    /// <param name="channel">The channel instance to wrap.</param>
    /// <param name="client">The Discord client for performing operations.</param>
    public GuildThreadChannelWrapper(DiscordClient client, Channel channel, IGuild guild)
        : base(client, channel)
    {
        Guild = guild;
    }

    public int? MessageCount => _channel.MessageCount;

    /// <inheritdoc />
    public int? MemberCount => _channel.MemberCount;
    public int? TotalMessageSent => _channel.TotalMessageSent;
    public ThreadAutoArchiveDuration? AutoArchiveDuration => _channel.ThreadMetadata?.AutoArchiveDuration;
    public Snowflake? OwnerId => _channel.OwnerId;
    public ThreadMetadata? Metadata => _channel.ThreadMetadata;
    public Snowflake[]? AppliedTags => _channel.AppliedTags;

    public IGuild Guild { get; }

    public bool Nsfw => _channel.Nsfw == true;

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

    public IRestAction<IGuildThreadChannel> AddThreadMember(Snowflake userId)
    {
        if (userId == default)
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

        return RestAction<IGuildThreadChannel>.Create(async cancellationToken =>
        {
            await _client.ChannelClient.AddThreadMemberAsync(_channel.Id, userId, cancellationToken);
            return this;
        });
    }

    public IRestAction<IGuildThreadChannel> RemoveThreadMember(Snowflake userId)
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
            return new GuildChannelUnionWrapper(_client, parent, Guild);
        });
    }

    public IRestAction<IMember[]> GetMembers()
    {
        return RestAction<IMember[]>.Create(async cancellationToken =>
        {
            var guildMembers = await Guild.GetMembers().ExecuteAsync(cancellationToken);
            var permissionContainer = GetPermissionContainer();

            return [.. guildMembers.Where(x => GetPermission(x).HasFlag(DiscordPermission.ViewChannel))];
        });
    }

    public ICreateInviteAction CreateInvite()
    {
        return new CreateInviteAction(_client, this);
    }

    public IRestAction<IReadOnlyList<IInvite>> RetrieveInvites()
    {
        return RestAction<IReadOnlyList<IInvite>>.Create(async cancellationToken =>
        {
            var invites = await _client.InviteClient.GetChannelInvitesAsync(_channel.Id, cancellationToken);
            return invites.Select(invite => new InviteWrapper(invite, this, _client)).Cast<IInvite>().ToList().AsReadOnly();
        });
    }

    public DiscordPermission GetPermission(IMember member)
    {
        return new ChannelPermissionCalculator(Guild, GetPermissionContainer()).GetPermission(member);
    }

    public IThreadChannelManager GetManager()
    {
        return new ThreadChannelManagerWrapper(Id, _client.ChannelClient);
    }
}
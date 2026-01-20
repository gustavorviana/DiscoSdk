using DiscoSdk.Hosting.EqualityComparers;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers.Channels;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;
using DiscoSdk.Utils;
using Channel = DiscoSdk.Models.Channels.Channel;

namespace DiscoSdk.Hosting.Wrappers;

internal class GuildWrapper : IGuild
{
    private readonly HashSet<Channel> _channels = new(new ChannelEqualityComparerById());
    private readonly object _updateLock = new();
    private readonly DiscordClient _client;
    private Guild _guild;

    public GuildWrapper(Guild guild, DiscordClient client)
    {
        _guild = guild ?? throw new ArgumentNullException(nameof(guild));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        RefreshProperties();

        foreach (var channel in _channels)
            _channels.Add(channel);
    }

    public Snowflake Id => _guild.Id;

    public string Name => _guild.Name;

    public DiscordImage? Icon { get; private set; }

    public DiscordImage? Splash { get; private set; }

    public DiscordImage? DiscoverySplash { get; private set; }

    public bool? Owner => _guild.Owner;

    public Snowflake? OwnerId => _guild.OwnerId;

    public DiscordPermission Permissions => (DiscordPermission)(_guild.Permissions ?? 0);

    public string? Region => _guild.Region;

    public Snowflake? AfkChannelId => _guild.AfkChannelId;

    public int? AfkTimeout => _guild.AfkTimeout;

    public bool? WidgetEnabled => _guild.WidgetEnabled;

    public Snowflake? WidgetChannelId => _guild.WidgetChannelId;

    public VerificationLevel? VerificationLevel => _guild.VerificationLevel;

    public DefaultMessageNotificationLevel? DefaultMessageNotifications => _guild.DefaultMessageNotifications;

    public ExplicitContentFilterLevel? ExplicitContentFilter => _guild.ExplicitContentFilter;

    public IRole[] Roles { get; private set; } = [];

    public IEmoji[]? Emojis { get; private set; } = [];

    public string[]? Features => _guild.Features;

    public MfaLevel? MfaLevel => _guild.MfaLevel;

    public Snowflake? ApplicationId => _guild.ApplicationId;

    public Snowflake? SystemChannelId => _guild.SystemChannelId;

    public SystemChannelFlags? SystemChannelFlags => _guild.SystemChannelFlags;

    public Snowflake? RulesChannelId => _guild.RulesChannelId;

    public int? MaxPresences => _guild.MaxPresences;

    public int? MaxMembers => _guild.MaxMembers;

    public string? VanityUrlCode => _guild.VanityUrlCode;

    public string? Description => _guild.Description;

    public DiscordImage? Banner { get; private set; }

    public PremiumTier? PremiumTier => _guild.PremiumTier;

    public int? PremiumSubscriptionCount => _guild.PremiumSubscriptionCount;

    public string? PreferredLocale => _guild.PreferredLocale;

    public Snowflake? PublicUpdatesChannelId => _guild.PublicUpdatesChannelId;

    public int? MaxVideoChannelUsers => _guild.MaxVideoChannelUsers;

    public int? ApproximateMemberCount => _guild.ApproximateMemberCount;

    public int? ApproximatePresenceCount => _guild.ApproximatePresenceCount;

    public bool? Unavailable => _guild.Unavailable;

    public IEditGuildAction Edit()
    {
        return new EditGuildAction(_client, this);
    }

    public IRestAction Delete()
    {
        return RestAction.Create(async cancellationToken =>
        {
            await _client.GuildClient.DeleteAsync(_guild.Id, cancellationToken);
        });
    }

    public IRestAction Leave()
    {
        return RestAction.Create(async cancellationToken =>
        {
            await _client.GuildClient.LeaveAsync(_guild.Id, cancellationToken);
        });
    }

    public ICreateChannelAction CreateChannel(string name, ChannelType type)
    {
        return new CreateChannelAction(_client, this, name, type);
    }

    public IRoleAction CreateRole()
    {
        return new RoleAction(_client, this);
    }

    public ICreateEmojiAction CreateEmoji(string name, DiscordImage image)
    {
        return new CreateEmojiAction(_client, this, name, image);
    }

    public IBanMemberAction BanMember(Snowflake userId, int deleteMessageDays = 0)
    {
        return new BanMemberAction(_client, _guild.Id, userId, deleteMessageDays);
    }

    public IRestAction UnbanMember(Snowflake userId)
    {
        return RestAction.Create(async cancellationToken =>
        {
            await _client.GuildClient.UnbanMemberAsync(_guild.Id, userId, cancellationToken);
        });
    }

    public IRestAction KickMember(Snowflake userId)
    {
        return RestAction.Create(async cancellationToken =>
        {
            await _client.GuildClient.KickMemberAsync(_guild.Id, userId, cancellationToken);
        });
    }

    public IMemberPaginationAction GetMembers()
    {
        return new MemberPaginationAction(_client, this);
    }

    public IRestAction<IMember?> GetMember(Snowflake userId)
    {
        return RestAction<IMember?>.Create(async cancellationToken =>
        {
            var member = await _client.GuildClient.GetMemberAsync(_guild.Id, userId, cancellationToken);
            if (member == null)
                return null;

            return new GuildMemberWrapper(_client, member, this);
        });
    }

    public IRestAction<IBan?> GetBan(Snowflake userId)
    {
        return RestAction<IBan?>.Create(async cancellationToken =>
        {
            var ban = await _client.GuildClient.GetBanAsync(_guild.Id, userId, cancellationToken);
            if (ban == null)
                return null;

            return new BanWrapper(ban, _client);
        });
    }

    public IAuditLogPaginationAction GetAuditLogs()
    {
        return new AuditLogPaginationAction(_client, this);
    }

    public IGuildChannelUnion? GetChannel(Snowflake channelId)
    {
        if (!_channels.TryGetValue(new Channel { Id = Id }, out var channel))
            return null;

        return new GuildChannelUnionWrapper(_client, channel, this);
    }

    public IGuildVoiceChannel? GetAfkChannel()
    {
        if (!AfkChannelId.HasValue || GetRawChannelById(AfkChannelId.Value) is not { } channel)
            return null;

        return new GuildVoiceChannelWrapper(_client, channel, this);
    }

    public IGuildTextChannel? GetSystemChannel()
    {
        return GetTextChannel(SystemChannelId);
    }

    public IGuildTextChannel? GetRulesChannel()
    {
        return GetTextChannel(RulesChannelId);
    }

    public IGuildTextChannel? GetPublicUpdatesChannel()
    {
        return GetTextChannel(PublicUpdatesChannelId);
    }

    private IGuildTextChannel? GetTextChannel(Snowflake? channelId)
    {
        var channel = GetRawChannelById(channelId);
        if (channel == null)
            return null;

        if (!ChannelTypeUtils.IsText(channel.Type))
            throw new InvalidCastException();

        return new GuildTextChannelWrapper(_client, channel, this);
    }

    private Channel? GetRawChannelById(Snowflake? channelId)
    {
        lock (_updateLock)
        {
            if (channelId == null)
                return null;

            if (!_channels.TryGetValue(new Channel { Id = channelId.Value }, out var channel))
                return null;

            return channel;
        }
    }

    public IReadOnlyList<IGuildChannelUnion> GetChannels()
    {
        lock (_updateLock)
        {
            return [.. _guild
                .Channels
                .Select(ch => new GuildChannelUnionWrapper(_client, ch, this))];
        }
    }

    public IReadOnlyList<IGuildTextChannel> GetTextChannels()
    {
        lock (_updateLock)
        {
            return [.._guild
                .Channels
                .Where(x => ChannelTypeUtils.IsText(x.Type))
                .Select(ch => ChannelWrapper.ToSpecificType(_client, ch, this))
                .OfType<IGuildTextChannel>()];
        }
    }

    public IReadOnlyList<IGuildVoiceChannel> GetVoiceChannels()
    {
        lock (_updateLock)
        {
            return [.._guild
                .Channels
                .Where(x => ChannelTypeUtils.IsVoice(x.Type))
                .Select(ch => new GuildVoiceChannelWrapper(_client, ch, this))];
        }
    }

    public IRestAction<IReadOnlyList<IRole>> GetRoles()
    {
        return RestAction<IReadOnlyList<IRole>>.Create(async cancellationToken =>
        {
            var roles = await _client.GuildClient.GetRolesAsync(_guild.Id, cancellationToken);
            return roles
                .Select(r => new RoleWrapper(_client, r, this))
                .Cast<IRole>()
                .ToList()
                .AsReadOnly();
        });
    }

    public IRestAction<IReadOnlyList<IInvite>> GetInvites()
    {
        return RestAction<IReadOnlyList<IInvite>>.Create(async cancellationToken =>
        {
            var invites = await _client.GuildClient.GetInvitesAsync(_guild.Id, cancellationToken);
            var result = new List<IInvite>();

            foreach (var invite in invites)
            {
                if (invite.Channel?.Id == null)
                    continue;

                var channel = GetChannel(invite.Channel.Id);
                if (channel is IGuildChannelBase guildChannel)
                    result.Add(new InviteWrapper(invite, guildChannel, _client));
            }

            return [.. result];
        });
    }

    public IRestAction<int> GetPruneCount(int days, params Snowflake[] includeRoles)
    {
        return RestAction<int>.Create(async cancellationToken =>
        {
            return await _client.GuildClient.GetPruneCountAsync(_guild.Id, days, includeRoles, cancellationToken);
        });
    }

    public IRestAction<int> BeginPrune(int days, params Snowflake[] includeRoles)
    {
        return RestAction<int>.Create(async cancellationToken =>
        {
            return await _client.GuildClient.BeginPruneAsync(_guild.Id, days, includeRoles, cancellationToken);
        });
    }

    public IRestAction<IReadOnlyList<VoiceRegion>> GetVoiceRegions()
    {
        return RestAction<IReadOnlyList<VoiceRegion>>.Create(async cancellationToken =>
        {
            var regions = await _client.GuildClient.GetVoiceRegionsAsync(_guild.Id, cancellationToken);
            return [.. regions];
        });
    }

    public IRestAction<GuildPreview> GetPreview()
    {
        return RestAction<GuildPreview>.Create(async cancellationToken =>
        {
            return await _client.GuildClient.GetPreviewAsync(_guild.Id, cancellationToken);
        });
    }

    public IRestAction<GuildWidget> GetWidget()
    {
        return RestAction<GuildWidget>.Create(async cancellationToken =>
        {
            return await _client.GuildClient.GetWidgetAsync(_guild.Id, cancellationToken);
        });
    }

    public IEditGuildWidgetAction EditWidget()
    {
        return new EditGuildWidgetAction(_client, this);
    }

    public IRestAction<WelcomeScreen> GetWelcomeScreen()
    {
        return RestAction<WelcomeScreen>.Create(async cancellationToken =>
        {
            return await _client.GuildClient.GetWelcomeScreenAsync(_guild.Id, cancellationToken);
        });
    }

    public IEditWelcomeScreenAction EditWelcomeScreen()
    {
        return new EditWelcomeScreenAction(_client, this);
    }

    public IRestAction<VanityUrl?> GetVanityUrl()
    {
        return RestAction<VanityUrl?>.Create(async token =>
        {
            return await _client.GuildClient.GetVanityUrlAsync(Id, token);
        });
    }

    public IRestAction<Stream> GetWidgetImage(string? style = null) => throw new NotSupportedException();

    internal void OnUpdate(Guild guild)
    {
        lock (_updateLock)
        {
            guild.Channels = _guild.Channels;
            _guild = guild;
            LoadImages();
        }
    }

    internal void OnChannelAdd(Channel channel)
    {
        lock (_updateLock)
        {
            _channels.Add(channel);
            _client.Channels.OnChannelCreated(ChannelWrapper.ToSpecificType(_client, channel, this));
        }
    }

    internal void OnChannelUpdate(Channel channel)
    {
        lock (_updateLock)
        {
            _channels.Remove(channel);
            _channels.Add(channel);
            _client.Channels.OnChannelUpdated(channel);
        }
    }

    internal void OnChannelDelete(Snowflake id)
    {
        lock (_updateLock)
        {
            _channels.Remove(new Channel { Id = id });
            _client.Channels.OnChannelRemoved(id);
        }
    }

    private void RefreshProperties()
    {
        Emojis = _guild.Emojis?.Select(x => new EmojiWrapper(_client, x, this))?.ToArray() ?? [];
        Roles = _guild.Roles?.Select(x => new RoleWrapper(_client, x, this))?.ToArray() ?? [];
        LoadImages();
    }

    private void LoadImages()
    {
        Icon = DiscordImage.FromBase64(_guild.Icon);
        Splash = DiscordImage.FromBase64(_guild.Splash);
        DiscoverySplash = DiscordImage.FromBase64(_guild.DiscoverySplash);
        Banner = DiscordImage.FromBase64(_guild.Banner);
    }
}
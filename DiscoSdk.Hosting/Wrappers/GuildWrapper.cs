using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers.Channels;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;
using DiscoSdk.Utils;
using Channel = DiscoSdk.Models.Channels.Channel;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Wrapper that implements <see cref="IGuild"/> for a <see cref="Guild"/> instance.
/// </summary>
internal class GuildWrapper : IGuild
{
    private readonly Guild _guild;
    private readonly DiscordClient _client;

    public GuildWrapper(Guild guild, DiscordClient client)
    {
        _guild = guild ?? throw new ArgumentNullException(nameof(guild));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        Roles = guild.Roles?.Select(x => new RoleWrapper(x, this, client))?.ToArray() ?? [];
    }

    public DiscordId Id => _guild.Id;

    public string Name => _guild.Name;

    public string? Icon => _guild.Icon;

    public string? IconHash => _guild.IconHash;

    public string? Splash => _guild.Splash;

    public string? DiscoverySplash => _guild.DiscoverySplash;

    public bool? Owner => _guild.Owner;

    public DiscordId? OwnerId => _guild.OwnerId;

    public DiscordPermission Permissions => (DiscordPermission)(_guild.Permissions ?? 0);

    public string? Region => _guild.Region;

    public DiscordId? AfkChannelId => _guild.AfkChannelId;

    public int? AfkTimeout => _guild.AfkTimeout;

    public bool? WidgetEnabled => _guild.WidgetEnabled;

    public DiscordId? WidgetChannelId => _guild.WidgetChannelId;

    public VerificationLevel? VerificationLevel => _guild.VerificationLevel;

    public DefaultMessageNotificationLevel? DefaultMessageNotifications => _guild.DefaultMessageNotifications;

    public ExplicitContentFilterLevel? ExplicitContentFilter => _guild.ExplicitContentFilter;

    public IRole[] Roles { get; private set; }

    public Emoji[]? Emojis => _guild.Emojis;

    public string[]? Features => _guild.Features;

    public MfaLevel? MfaLevel => _guild.MfaLevel;

    public DiscordId? ApplicationId => _guild.ApplicationId;

    public DiscordId? SystemChannelId => _guild.SystemChannelId;

    public SystemChannelFlags? SystemChannelFlags => _guild.SystemChannelFlags;

    public DiscordId? RulesChannelId => _guild.RulesChannelId;

    public int? MaxPresences => _guild.MaxPresences;

    public int? MaxMembers => _guild.MaxMembers;

    public string? VanityUrlCode => _guild.VanityUrlCode;

    public string? Description => _guild.Description;

    public string? Banner => _guild.Banner;

    public PremiumTier? PremiumTier => _guild.PremiumTier;

    public int? PremiumSubscriptionCount => _guild.PremiumSubscriptionCount;

    public string? PreferredLocale => _guild.PreferredLocale;

    public DiscordId? PublicUpdatesChannelId => _guild.PublicUpdatesChannelId;

    public int? MaxVideoChannelUsers => _guild.MaxVideoChannelUsers;

    public int? ApproximateMemberCount => _guild.ApproximateMemberCount;

    public int? ApproximatePresenceCount => _guild.ApproximatePresenceCount;

    public bool? Unavailable => _guild.Unavailable;

    public IEditGuildRestAction Edit() => throw new NotSupportedException();

    public IRestAction Delete() => throw new NotSupportedException();

    public IRestAction Leave() => throw new NotSupportedException();

    public ICreateChannelAction CreateChannel(string name, ChannelType type) => throw new NotSupportedException();

    public IRoleAction CreateRole() => throw new NotSupportedException();

    public ICreateEmojiAction CreateEmoji(string name, byte[] image) => throw new NotSupportedException();

    public IBanMemberAction BanMember(DiscordId userId, int deleteMessageDays = 0) => throw new NotSupportedException();

    public IRestAction UnbanMember(DiscordId userId) => throw new NotSupportedException();

    public IRestAction KickMember(DiscordId userId) => throw new NotSupportedException();

    public IMemberPaginationAction GetMembers()
    {
        return new MemberPaginationAction(_client, this);
    }

    public IRestAction<IMember?> GetMember(DiscordId userId)
    {
        return RestAction<IMember?>.Create(async cancellationToken =>
        {
            var member = await _client.GuildClient.GetMemberAsync(_guild.Id, userId, cancellationToken);
            if (member == null)
                return null;

            return new GuildMemberWrapper(member, this, _client);
        });
    }

    public IBanPaginationAction GetBans() => throw new NotSupportedException();

    public IRestAction<Ban?> GetBan(DiscordId userId)
    {
        return RestAction<Ban?>.Create(async cancellationToken =>
        {
            var ban = await _client.GuildClient.GetBanAsync(_guild.Id, userId, cancellationToken);
            return ban;
        });
    }

    public IAuditLogPaginationAction GetAuditLogs() => throw new NotSupportedException();

    public IRestAction<IGuildChannelUnion?> GetChannel(DiscordId channelId)
    {
        return RestAction<IGuildChannelUnion?>.Create(async cancellationToken =>
        {
            var channel = await GetRawChannel(channelId, cancellationToken);
            if (channel == null)
                return null;

            if (channel.GuildId != _guild.Id)
                return null;

            return new GuildChannelUnionWrapper(channel, this, _client);
        });
    }

    public IRestAction<IGuildVoiceChannel?> GetAfkChannel()
    {
        if (!AfkChannelId.HasValue)
            return RestAction<IGuildVoiceChannel?>.FromResult(null);

        return RestAction<IGuildVoiceChannel?>.Create(async cancellationToken =>
        {
            var channel = await GetRawChannel(AfkChannelId.Value, cancellationToken);
            if (channel == null)
                return null;

            if (channel.GuildId != _guild.Id)
                return null;

            return new GuildVoiceChannelWrapper(channel, this, _client);
        });
    }

    public IRestAction<IGuildTextChannel?> GetSystemChannel()
    {
        return GetChannelAction(SystemChannelId);
    }

    public IRestAction<IGuildTextChannel?> GetRulesChannel()
    {
        return GetChannelAction(RulesChannelId);
    }

    public IRestAction<IGuildTextChannel?> GetPublicUpdatesChannel()
    {
        return GetChannelAction(PublicUpdatesChannelId);
    }

    private IRestAction<IGuildTextChannel?> GetChannelAction(DiscordId? channelId)
    {
        if (channelId == null)
            return RestAction<IGuildTextChannel?>.FromResult(null);

        return RestAction<IGuildTextChannel?>.Create(async cancellationToken =>
        {
            var channel = await GetRawChannel(channelId.Value, cancellationToken);
            if (channel == null)
                return null;

            return new GuildTextChannelWrapper(channel, this, _client);
        });
    }

    private async Task<Channel> GetRawChannel(DiscordId channelId, CancellationToken cancellationToken)
    {
        return await _client.ChannelClient.GetAsync(channelId, cancellationToken);
    }

    public IRestAction<IReadOnlyList<IGuildChannelUnion>> GetChannels()
    {
        return RestAction<IReadOnlyList<IGuildChannelUnion>>.Create(async cancellationToken =>
        {
            var channels = await _client.GuildClient.GetChannelsAsync(_guild.Id, cancellationToken);

            return [.. channels.Select(ch => new GuildChannelUnionWrapper(ch, this, _client))];
        });
    }

    public IRestAction<IReadOnlyList<IGuildTextChannel>> GetTextChannels()
    {
        return RestAction<IReadOnlyList<IGuildTextChannel>>.Create(async cancellationToken =>
        {
            var channels = await _client.GuildClient.GetChannelsAsync(_guild.Id, cancellationToken);
            return [..channels
                .Where(x => ChannelTypeUtils.IsText(x.Type))
                .Select(ch => ChannelWrapper.ToSpecificType(ch, this, _client))
                .OfType<IGuildTextChannel>()];
        });
    }

    public IRestAction<IReadOnlyList<IGuildVoiceChannel>> GetVoiceChannels()
    {
        return RestAction<IReadOnlyList<IGuildVoiceChannel>>.Create(async cancellationToken =>
        {
            var channels = await _client.GuildClient.GetChannelsAsync(_guild.Id, cancellationToken);
            return [..channels
                .Where(x => ChannelTypeUtils.IsVoice(x.Type))
                .Select(ch => new GuildVoiceChannelWrapper(ch, this, _client))];
        });
    }

    public IRestAction<IReadOnlyList<IRole>> GetRoles()
    {
        return RestAction<IReadOnlyList<IRole>>.Create(async cancellationToken =>
        {
            var roles = await _client.GuildClient.GetRolesAsync(_guild.Id, cancellationToken);
            return roles
                .Select(r => new RoleWrapper(r, this, _client))
                .Cast<IRole>()
                .ToList()
                .AsReadOnly();
        });
    }

    public IRestAction<IReadOnlyList<Emoji>> GetEmojis() => throw new NotSupportedException();

    public IRestAction<IReadOnlyList<IInvite>> GetInvites() => throw new NotSupportedException();

    public IRestAction<int> GetPruneCount(int days, params DiscordId[] includeRoles) => throw new NotSupportedException();

    public IBeginPruneAction BeginPrune(int days, params DiscordId[] includeRoles) => throw new NotSupportedException();

    public IRestAction<IReadOnlyList<VoiceRegion>> GetVoiceRegions() => throw new NotSupportedException();

    public IRestAction<GuildPreview> GetPreview() => throw new NotSupportedException();

    public IRestAction<GuildWidget> GetWidget() => throw new NotSupportedException();

    public IEditGuildWidgetAction EditWidget() => throw new NotSupportedException();

    public IRestAction<WelcomeScreen> GetWelcomeScreen() => throw new NotSupportedException();

    public IEditWelcomeScreenAction EditWelcomeScreen() => throw new NotSupportedException();

    public IRestAction<string?> GetVanityUrl() => throw new NotSupportedException();

    public IRestAction<Stream> GetWidgetImage(string? style = null) => throw new NotSupportedException();
}



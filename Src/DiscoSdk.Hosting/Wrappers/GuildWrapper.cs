using DiscoSdk.Caching;
using DiscoSdk.Hosting.EqualityComparers;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Surfaces;
using DiscoSdk.Hosting.Wrappers.Channels;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;
using Channel = DiscoSdk.Models.Channels.Channel;

namespace DiscoSdk.Hosting.Wrappers;

internal class GuildWrapper : IGuild
{
    private readonly HashSet<Channel> _channels = new(new ChannelEqualityComparerById());
    private readonly object _updateLock = new();
    private readonly DiscordClient _client;
    private Guild _guild;

    /// <summary>
    /// Hosting-internal raw POCO. Surfaces (<c>GuildChannelsSurface</c>,
    /// <c>GuildWidgetSurfaceImpl</c>, …) read fields straight off this rather than mirror every
    /// property on the wrapper.
    /// </summary>
    internal Guild Data => _guild;

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

    public DiscordImageUrl? Icon { get; private set; }

    public DiscordImageUrl? Splash { get; private set; }

    public DiscordImageUrl? DiscoverySplash { get; private set; }

    public bool Owner => _guild.Owner ?? false;

    public Snowflake OwnerId => _guild.OwnerId ?? default;

    public DiscordPermission Permissions => (DiscordPermission)(_guild.Permissions ?? 0);

    public string? Region => _guild.Region;

    public VerificationLevel VerificationLevel => _guild.VerificationLevel ?? VerificationLevel.None;

    public DefaultMessageNotificationLevel DefaultMessageNotifications => _guild.DefaultMessageNotifications ?? DefaultMessageNotificationLevel.AllMessages;

    public ExplicitContentFilterLevel ExplicitContentFilter => _guild.ExplicitContentFilter ?? ExplicitContentFilterLevel.Disabled;

    internal IRole[] CachedRolesSnapshot { get; private set; } = [];
    internal IEmoji[] CachedEmojisSnapshot { get; private set; } = [];

    public string[] Features => _guild.Features ?? [];

    public MfaLevel MfaLevel => _guild.MfaLevel ?? MfaLevel.None;

    public Snowflake? ApplicationId => _guild.ApplicationId;

    public int? MaxPresences => _guild.MaxPresences;

    public int? MaxMembers => _guild.MaxMembers;

    public string? VanityUrlCode => _guild.VanityUrlCode;

    public string? Description => _guild.Description;

    public DiscordImageUrl? Banner { get; private set; }

    public PremiumTier PremiumTier => _guild.PremiumTier ?? PremiumTier.None;

    public int? PremiumSubscriptionCount => _guild.PremiumSubscriptionCount;

    public string PreferredLocale => _guild.PreferredLocale ?? "en-US";

    public int? MaxVideoChannelUsers => _guild.MaxVideoChannelUsers;

    public GuildHubType? HubType => _guild.HubType;

    public int? ApproximateMemberCount => _guild.ApproximateMemberCount;

    public int? ApproximatePresenceCount => _guild.ApproximatePresenceCount;

    public bool Unavailable => _guild.Unavailable ?? false;

    public IEditGuildAction Edit()
    {
        return new EditGuildAction(_client, this);
    }

    public IReasonedRestAction Delete()
    {
        return new ReasonedRestAction((_, cancellationToken) =>
            _client.GuildClient.DeleteAsync(_guild.Id, cancellationToken));
    }

    public IRestAction Leave()
    {
        return RestAction.Create(async cancellationToken =>
        {
            await _client.GuildClient.LeaveAsync(_guild.Id, cancellationToken);
        });
    }

    public IAuditLogPaginationAction GetAuditLogs()
    {
        return new AuditLogPaginationAction(_client, this);
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

    internal Channel? RawChannelById(Snowflake? channelId) => GetRawChannelById(channelId);
    internal IReadOnlyList<Channel> ChannelsSnapshot()
    {
        lock (_updateLock) return [.. _guild.Channels];
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

                var channel = Channels.Get(invite.Channel.Id);
                if (channel is IGuildChannelBase guildChannel)
                    result.Add(new InviteWrapper(invite, guildChannel, _client));
            }

            return [.. result];
        });
    }

    public IRestAction<IReadOnlyList<IVoiceRegion>> GetVoiceRegions()
    {
        return RestAction<IReadOnlyList<IVoiceRegion>>.Create(async cancellationToken =>
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

    public IRestAction<IVanityUrl?> GetVanityUrl()
    {
        return RestAction<IVanityUrl?>.Create(async token =>
        {
            return await _client.GuildClient.GetVanityUrlAsync(Id, token);
        });
    }


    public IReasonedRestAction ModifyMfaLevel(MfaLevel level)
        => new ReasonedRestAction((reason, ct) => _client.GuildClient.ModifyMfaLevelAsync(_guild.Id, level, reason, ct));

    public IRestAction<IReadOnlyList<IIntegration>> GetIntegrations()
        => RestAction<IReadOnlyList<IIntegration>>.Create(async ct =>
        {
            var integrations = await _client.GuildClient.ListIntegrationsAsync(_guild.Id, ct);
            return integrations.Select(i => (IIntegration)new IntegrationWrapper(_client, _guild.Id, i)).ToList().AsReadOnly();
        });

    public IReasonedRestAction<IIncidentsData> ModifyIncidentActions(DateTimeOffset? invitesDisabledUntil, DateTimeOffset? dmsDisabledUntil)
        => new ReasonedRestAction<IIncidentsData>((reason, ct) => _client.GuildClient.ModifyIncidentActionsAsync(_guild.Id, invitesDisabledUntil, dmsDisabledUntil, reason, ct));

    public IRestAction<IReadOnlyList<IWebhook>> GetWebhooks()
        => RestAction<IReadOnlyList<IWebhook>>.Create(async ct =>
        {
            var webhooks = await _client.WebhookClient.GetGuildWebhooksAsync(_guild.Id, ct);
            return webhooks.Select(w => (IWebhook)new WebhookWrapper(_client, w)).ToList().AsReadOnly();
        });

    public IGuildCommands Commands => _commands ??= new GuildCommandsSurface(_client, _guild.Id);
    private IGuildCommands? _commands;

    public IGuildMembers Members => _members ??= _client.MembersInternal.OfGuild(_guild.Id, this);
    private IGuildMembers? _members;

    public IGuildBans Bans => _bans ??= new GuildBansSurface(_client, _guild.Id);
    private IGuildBans? _bans;

    public IGuildChannels Channels => _channelsSurface ??= new GuildChannelsSurface(_client, this);
    private IGuildChannels? _channelsSurface;

    public IGuildRoles Roles => _rolesSurface ??= new GuildRolesSurface(_client, this);
    private IGuildRoles? _rolesSurface;

    public IGuildScheduledEvents ScheduledEvents => _scheduledEvents ??= new GuildScheduledEventsSurface(_client, _guild.Id);
    private IGuildScheduledEvents? _scheduledEvents;

    public IGuildAutoModeration AutoModeration => _autoMod ??= new GuildAutoModerationSurface(_client, _guild.Id);
    private IGuildAutoModeration? _autoMod;

    public IGuildWidgetSurface Widget => _widget ??= new GuildWidgetSurfaceImpl(_client, this);
    private IGuildWidgetSurface? _widget;

    public IGuildWelcomeScreen WelcomeScreen => _welcomeScreen ??= new GuildWelcomeScreenSurface(_client, this);
    private IGuildWelcomeScreen? _welcomeScreen;

    public IGuildOnboardingSurface Onboarding => _onboarding ??= new GuildOnboardingSurfaceImpl(_client, _guild.Id);
    private IGuildOnboardingSurface? _onboarding;

    public IGuildTemplates Templates => _templates ??= new GuildTemplatesSurface(_client, _guild.Id);
    private IGuildTemplates? _templates;

    public IGuildPrune Prune => _prune ??= new GuildPruneSurface(_client, _guild.Id);
    private IGuildPrune? _prune;

    public IGuildEmojis Emojis => _emojisSurface ??= new GuildEmojisSurface(_client, this);
    private IGuildEmojis? _emojisSurface;

    public IGuildSoundboard Soundboard => _soundboard ??= new GuildSoundboardSurface(_client, _guild.Id);
    private IGuildSoundboard? _soundboard;

    public IGuildStickers Stickers => _stickerScope ??= _client.Stickers.OfGuild(_guild.Id);
    private IGuildStickers? _stickerScope;

    internal void OnUpdate(Guild guild)
    {
        lock (_updateLock)
        {
            // Channel snapshot lives on the wrapper, not on the GUILD_UPDATE payload — Discord
            // never re-sends the channel list on update, so carry it over.
            guild.Channels = _guild.Channels;
            _guild = guild;
            RefreshProperties();
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
        CachedEmojisSnapshot = _guild.Emojis?.Select(x => new EmojiWrapper(_client, x, this))?.ToArray() ?? [];
        CachedRolesSnapshot = _guild.Roles?.Select(x => new RoleWrapper(_client, x, this))?.ToArray() ?? [];
        LoadImages();
    }

    private void LoadImages()
    {
        Icon = DiscordImageUrl.ParseIcon(_guild.Id, _guild.Icon);
        Splash = DiscordImageUrl.ParseSplash(_guild.Id, _guild.Splash);
        DiscoverySplash = DiscordImageUrl.ParseDiscoverySplash(_guild.Id, _guild.DiscoverySplash);
        Banner = DiscordImageUrl.ParseBanner(_guild.Id, _guild.Banner);
    }
}
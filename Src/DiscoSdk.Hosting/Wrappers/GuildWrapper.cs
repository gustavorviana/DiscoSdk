using DiscoSdk.Hosting.EqualityComparers;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers.Channels;
using DiscoSdk.Models;
using DiscoSdk.Models.AutoModeration;
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

    public DiscordImageUrl? Icon { get; private set; }

    public DiscordImageUrl? Splash { get; private set; }

    public DiscordImageUrl? DiscoverySplash { get; private set; }

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

    public DiscordImageUrl? Banner { get; private set; }

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

    public ICreateEmojiAction CreateEmoji(string name, DiscordImageBuffer image)
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

    public IRestAction<IReadOnlyList<IAutoModerationRule>> GetAutoModerationRules()
        => RestAction<IReadOnlyList<IAutoModerationRule>>.Create(async ct =>
        {
            var rules = await _client.AutoModerationClient.ListRulesAsync(_guild.Id, ct);
            return rules.Select(r => (IAutoModerationRule)new AutoModerationRuleWrapper(_client, r)).ToList().AsReadOnly();
        });

    public IRestAction<IAutoModerationRule> GetAutoModerationRule(Snowflake ruleId)
        => RestAction<IAutoModerationRule>.Create(async ct => new AutoModerationRuleWrapper(_client, await _client.AutoModerationClient.GetRuleAsync(_guild.Id, ruleId, ct)));

    public ICreateAutoModerationRuleAction CreateAutoModerationRule(string name, AutoModerationEventType eventType, AutoModerationTriggerType triggerType)
        => new CreateAutoModerationRuleAction(_client, _guild.Id, name, eventType, triggerType);

    public IRestAction<IReadOnlyList<IGuildScheduledEvent>> GetScheduledEvents(bool? withUserCount = null)
        => RestAction<IReadOnlyList<IGuildScheduledEvent>>.Create(async ct =>
        {
            var events = await _client.GuildScheduledEventClient.ListAsync(_guild.Id, withUserCount, ct);
            return events.Select(e => (IGuildScheduledEvent)new GuildScheduledEventWrapper(_client, e)).ToList().AsReadOnly();
        });

    public IRestAction<IGuildScheduledEvent> GetScheduledEvent(Snowflake eventId, bool? withUserCount = null)
        => RestAction<IGuildScheduledEvent>.Create(async ct =>
            new GuildScheduledEventWrapper(_client, await _client.GuildScheduledEventClient.GetAsync(_guild.Id, eventId, withUserCount, ct)));

    public ICreateScheduledEventAction CreateScheduledEvent(
        string name,
        DateTimeOffset scheduledStartTime,
        ScheduledEventEntityType entityType)
        => new CreateScheduledEventAction(_client, _guild.Id, name, scheduledStartTime, entityType);

    public IRestAction<IReadOnlyList<ISticker>> GetStickers()
        => RestAction<IReadOnlyList<ISticker>>.Create(async ct =>
        {
            var stickers = await _client.StickerClient.ListGuildStickersAsync(_guild.Id, ct);
            return stickers.Select(s => (ISticker)new StickerWrapper(_client, s)).ToList().AsReadOnly();
        });

    public IRestAction<ISticker> GetSticker(Snowflake stickerId)
        => RestAction<ISticker>.Create(async ct =>
            new StickerWrapper(_client, await _client.StickerClient.GetGuildStickerAsync(_guild.Id, stickerId, ct)));

    public ICreateGuildStickerAction CreateSticker(string name, string tags, DiscoSdk.Models.Messages.MessageFile file)
        => new CreateGuildStickerAction(_client, _guild.Id, name, tags, file);

    public IRequestGuildMembersAction RequestMembers()
        => new RequestGuildMembersAction(_client, _guild.Id);

    public IRestAction<IGuildOnboarding> GetOnboarding()
        => RestAction<IGuildOnboarding>.Create(async ct => new GuildOnboardingWrapper(_client, await _client.GuildTemplateClient.GetOnboardingAsync(_guild.Id, ct)));

    public IRestAction<IReadOnlyList<IGuildTemplate>> GetTemplates()
        => RestAction<IReadOnlyList<IGuildTemplate>>.Create(async ct =>
        {
            var templates = await _client.GuildTemplateClient.GetGuildTemplatesAsync(_guild.Id, ct);
            return templates.Select(t => (IGuildTemplate)new GuildTemplateWrapper(_client, t)).ToList().AsReadOnly();
        });

    public IRestAction<IGuildTemplate> CreateTemplate(string name, string? description = null)
        => RestAction<IGuildTemplate>.Create(async ct => new GuildTemplateWrapper(_client, await _client.GuildTemplateClient.CreateGuildTemplateAsync(_guild.Id, name, description, ct)));

    public IBanPaginationAction GetBans()
        => new BanPaginationAction(_client, _guild.Id);

    public IRestAction<IReadOnlyList<Snowflake>> BulkBan(IEnumerable<Snowflake> userIds, int? deleteMessageSeconds = null)
    {
        ArgumentNullException.ThrowIfNull(userIds);
        return RestAction<IReadOnlyList<Snowflake>>.Create(async ct =>
        {
            var response = await _client.GuildClient.BulkBanAsync(_guild.Id, userIds, deleteMessageSeconds, ct);
            var banned = new List<Snowflake>();
            if (response.ValueKind == System.Text.Json.JsonValueKind.Object &&
                response.TryGetProperty("banned_users", out var bannedArr) &&
                bannedArr.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var item in bannedArr.EnumerateArray())
                {
                    if (item.ValueKind == System.Text.Json.JsonValueKind.String &&
                        Snowflake.TryParse(item.GetString()!, out var id))
                        banned.Add(id);
                }
            }
            return banned.AsReadOnly();
        });
    }

    public IRestAction<IReadOnlyList<IMember>> SearchMembers(string query, int? limit = null)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be null or empty.", nameof(query));
        return RestAction<IReadOnlyList<IMember>>.Create(async ct =>
        {
            var members = await _client.GuildClient.SearchMembersAsync(_guild.Id, query, limit, ct);
            return members
                .Where(m => m.User != null)
                .Select(m => (IMember)new GuildMemberWrapper(_client, m, this))
                .ToList()
                .AsReadOnly();
        });
    }

    public IRestAction<IMember?> AddMember(Snowflake userId, string accessToken, string? nick = null, IEnumerable<Snowflake>? roles = null, bool? mute = null, bool? deaf = null)
    {
        return RestAction<IMember?>.Create(async ct =>
        {
            var member = await _client.GuildClient.AddMemberAsync(_guild.Id, userId, accessToken, nick, roles, mute, deaf, ct);
            return member is null ? null : new GuildMemberWrapper(_client, member, this);
        });
    }

    public IRestAction<IMember> ModifyCurrentMember(string? nick)
    {
        return RestAction<IMember>.Create(async ct =>
        {
            var member = await _client.GuildClient.ModifyCurrentMemberAsync(_guild.Id, nick, ct);
            return new GuildMemberWrapper(_client, member, this);
        });
    }

    public IRestAction<IMember> ModifyMember(Snowflake userId, string? nick = null, IEnumerable<Snowflake>? roles = null, bool? mute = null, bool? deaf = null, Snowflake? channelId = null, DateTimeOffset? communicationDisabledUntil = null, int? flags = null)
    {
        return RestAction<IMember>.Create(async ct =>
        {
            var body = new Dictionary<string, object?>();
            if (nick is not null) body["nick"] = nick;
            if (roles is not null) body["roles"] = roles.Select(r => r.ToString()).ToArray();
            if (mute.HasValue) body["mute"] = mute.Value;
            if (deaf.HasValue) body["deaf"] = deaf.Value;
            if (channelId.HasValue) body["channel_id"] = channelId.Value == default ? null : channelId.Value.ToString();
            if (communicationDisabledUntil.HasValue)
                body["communication_disabled_until"] = communicationDisabledUntil.Value == DateTimeOffset.MinValue
                    ? null
                    : communicationDisabledUntil.Value.ToString("o");
            if (flags.HasValue) body["flags"] = flags.Value;

            var member = await _client.GuildClient.ModifyMemberAsync(_guild.Id, userId, body, ct);
            return new GuildMemberWrapper(_client, member, this);
        });
    }

    public IRestAction AddMemberRole(Snowflake userId, Snowflake roleId)
        => RestAction.Create(ct => _client.GuildClient.AddMemberRoleAsync(_guild.Id, userId, roleId, ct));

    public IRestAction RemoveMemberRole(Snowflake userId, Snowflake roleId)
        => RestAction.Create(ct => _client.GuildClient.RemoveMemberRoleAsync(_guild.Id, userId, roleId, ct));

    public IRestAction ModifyMfaLevel(MfaLevel level)
        => RestAction.Create(ct => _client.GuildClient.ModifyMfaLevelAsync(_guild.Id, level, ct));

    public IRestAction ModifyChannelPositions(IEnumerable<ChannelPosition> positions)
    {
        ArgumentNullException.ThrowIfNull(positions);
        return RestAction.Create(ct =>
        {
            var payload = positions.Select(p => new Dictionary<string, object?>
            {
                ["id"] = p.Id.ToString(),
                ["position"] = p.Position,
                ["lock_permissions"] = p.LockPermissions,
                ["parent_id"] = p.ParentId.HasValue ? p.ParentId.Value.ToString() : null
            });
            return _client.GuildClient.ModifyChannelPositionsAsync(_guild.Id, payload, ct);
        });
    }

    public IRestAction<IReadOnlyList<IIntegration>> GetIntegrations()
        => RestAction<IReadOnlyList<IIntegration>>.Create(async ct =>
        {
            var integrations = await _client.GuildClient.ListIntegrationsAsync(_guild.Id, ct);
            return integrations.Select(i => (IIntegration)new IntegrationWrapper(_client, _guild.Id, i)).ToList().AsReadOnly();
        });

    public IRestAction<IncidentsData> ModifyIncidentActions(DateTimeOffset? invitesDisabledUntil, DateTimeOffset? dmsDisabledUntil)
        => RestAction<IncidentsData>.Create(ct => _client.GuildClient.ModifyIncidentActionsAsync(_guild.Id, invitesDisabledUntil, dmsDisabledUntil, ct));

    public IRestAction<IReadOnlyList<IWebhook>> GetWebhooks()
        => RestAction<IReadOnlyList<IWebhook>>.Create(async ct =>
        {
            var webhooks = await _client.WebhookClient.GetGuildWebhooksAsync(_guild.Id, ct);
            return webhooks.Select(w => (IWebhook)new WebhookWrapper(_client, w)).ToList().AsReadOnly();
        });

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
        Icon = DiscordImageUrl.ParseIcon(_guild.Id, _guild.Icon);
        Splash = DiscordImageUrl.ParseSplash(_guild.Id, _guild.Splash);
        DiscoverySplash = DiscordImageUrl.ParseDiscoverySplash(_guild.Id, _guild.DiscoverySplash);
        Banner = DiscordImageUrl.ParseBanner(_guild.Id, _guild.Banner);
    }
}
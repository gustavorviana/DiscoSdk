using DiscoSdk.Hosting.Managers;
using DiscoSdk.Models;
using DiscoSdk.Models.Activities;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using System.Collections.Immutable;
using System.Globalization;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Wrapper that implements <see cref="IMember"/> for a <see cref="GuildMember"/> instance.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GuildMemberWrapper"/> class.
/// </remarks>
/// <param name="member">The guild member instance to wrap.</param>
/// <param name="guild">The guild this member belongs to.</param>
/// <param name="client">The Discord client for performing operations.</param>
internal class GuildMemberWrapper(DiscordClient client, GuildMember member, IGuild guild) : IMember
{
    private readonly GuildMember _member = member ?? throw new ArgumentNullException(nameof(member));
    private readonly IGuild _guild = guild ?? throw new ArgumentNullException(nameof(guild));

    /// <summary>
    /// Underlying POCO model. Accessed by the SDK cache layer to persist the member without
    /// exposing the wrapper.
    /// </summary>
    internal GuildMember Model => _member;
    private IUser? _user;
    private ImmutableList<IRole>? _roles;
    private ImmutableHashSet<IRole>? _unsortedRoles;
    private RoleColors? _colors;

    // IMentionable / IWithDiscordIdentity
    /// <inheritdoc />
    public Snowflake Id => _member.User?.UserId ?? default;

    /// <inheritdoc />
    public DateTimeOffset CreatedAt => Id.CreatedAt;

    // IChannel
    /// <inheritdoc />
    public string Name => EffectiveName;


    // IMember properties
    /// <inheritdoc />
    public IUser User
    {
        get
        {
            if (_user == null && _member.User != null)
                _user = new UserWrapper(client, _member.User);

            return _user ?? throw new InvalidOperationException("Member has no user data.");
        }
    }

    /// <inheritdoc />
    public IGuild Guild => _guild;

    /// <inheritdoc />
    public DateTimeOffset? TimeJoined
    {
        get
        {
            if (string.IsNullOrEmpty(_member.JoinedAt))
                return null;
            return DateTimeOffset.TryParse(_member.JoinedAt, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var result) ? result : null;
        }
    }

    /// <inheritdoc />
    public DateTimeOffset? TimeBoosted
    {
        get
        {
            if (string.IsNullOrEmpty(_member.PremiumSince))
                return null;
            return DateTimeOffset.TryParse(_member.PremiumSince, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var result) ? result : null;
        }
    }

    /// <inheritdoc />
    public DateTimeOffset? TimeOutEnd
    {
        get
        {
            if (string.IsNullOrEmpty(_member.CommunicationDisabledUntil))
                return null;
            return DateTimeOffset.TryParse(_member.CommunicationDisabledUntil, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var result) ? result : null;
        }
    }

    /// <inheritdoc />
    public IGuildVoiceState? VoiceState => null;

    /// <inheritdoc />
    public IActivity[] Activities
        => client.Presences.TryGet(_guild.Id, Id)?.Activities ?? [];

    /// <inheritdoc />
    public OnlineStatus OnlineStatus
        => PresenceManager.MapStatus(client.Presences.TryGet(_guild.Id, Id)?.Status) ?? OnlineStatus.Offline;

    /// <inheritdoc />
    public OnlineStatus? GetOnlineStatus(ClientType clientType)
    {
        var clientStatus = client.Presences.TryGetClientStatus(_guild.Id, Id);
        if (clientStatus is null)
            return null;

        var raw = clientType switch
        {
            ClientType.Desktop => clientStatus.Desktop,
            ClientType.Mobile => clientStatus.Mobile,
            ClientType.Web => clientStatus.Web,
            _ => null
        };

        return PresenceManager.MapStatus(raw);
    }

    /// <inheritdoc />
    public ImmutableHashSet<ClientType> ActiveClients
    {
        get
        {
            var clientStatus = client.Presences.TryGetClientStatus(_guild.Id, Id);
            if (clientStatus is null)
                return ImmutableHashSet<ClientType>.Empty;

            var builder = ImmutableHashSet.CreateBuilder<ClientType>();
            if (!string.IsNullOrEmpty(clientStatus.Desktop)) builder.Add(ClientType.Desktop);
            if (!string.IsNullOrEmpty(clientStatus.Mobile)) builder.Add(ClientType.Mobile);
            if (!string.IsNullOrEmpty(clientStatus.Web)) builder.Add(ClientType.Web);
            return builder.ToImmutable();
        }
    }

    /// <inheritdoc />
    public string? Nickname => _member.Nick;

    /// <inheritdoc />
    public string EffectiveName => Nickname ?? _member.User?.Username ?? "UNKNOW";

    /// <inheritdoc />
    public string? AvatarId => _member.Avatar;

    /// <inheritdoc />
    public string? AvatarUrl
    {
        get
        {
            if (string.IsNullOrEmpty(_member.Avatar))
                return null;

            var extension = _member.Avatar.StartsWith("a_") ? "gif" : "png";
            return $"https://cdn.discordapp.com/guilds/{_guild.Id}/users/{Id}/avatars/{_member.Avatar}.{extension}";
        }
    }

    /// <inheritdoc />
    public ImmutableList<IRole> Roles
    {
        get
        {
            if (_roles == null)
            {
                var roleList = new List<IRole>();
                if (_member.Roles != null)
                {
                    var cached = _guild.Roles.GetCached();
                    foreach (var roleIdStr in _member.Roles)
                    {
                        if (Snowflake.TryParse(roleIdStr, out var roleId))
                        {
                            var role = cached.FirstOrDefault(r => r.Id == roleId);
                            if (role != null)
                                roleList.Add(role);
                        }
                    }
                }
                _roles = roleList.OrderByDescending(r => r.Position).ToImmutableList();
            }
            return _roles;
        }
    }

    /// <inheritdoc />
    public ImmutableHashSet<IRole> UnsortedRoles
    {
        get
        {
            if (_unsortedRoles == null)
            {
                var roleSet = new HashSet<IRole>();
                if (_member.Roles != null)
                {
                    var cached = _guild.Roles.GetCached();
                    foreach (var roleIdStr in _member.Roles)
                    {
                        if (Snowflake.TryParse(roleIdStr, out var roleId))
                        {
                            var role = cached.FirstOrDefault(r => r.Id == roleId);
                            if (role != null)
                                roleSet.Add(role);
                        }
                    }
                }
                _unsortedRoles = roleSet.ToImmutableHashSet();
            }
            return _unsortedRoles;
        }
    }

    /// <inheritdoc />
    public RoleColors Colors
    {
        get
        {
            if (_colors == null)
            {
                // Get the highest role with a color (non-default)
                var defaultColor = new Color(IRole.DefaultColorRaw);
                var coloredRole = Roles.FirstOrDefault(r => r.Colors.Value != defaultColor.Value);
                _colors = new RoleColors(coloredRole?.Colors ?? defaultColor);
            }
            return _colors;
        }
    }

    /// <inheritdoc />
    public int FlagsRaw => (int)(_member.Flags ?? GuildMemberFlags.None);

    /// <inheritdoc />
    public ImmutableHashSet<MemberFlag> Flags => MemberFlagExtensions.FromRaw(FlagsRaw);

    /// <inheritdoc />
    public bool IsOwner => _guild.OwnerId == Id;

    /// <inheritdoc />
    public bool IsPending => _member.Pending ?? false;

    /// <inheritdoc />
    public IChannel? DefaultChannel => null;

    /// <inheritdoc />
    public bool CanInteract(IMember member)
    {
        ArgumentNullException.ThrowIfNull(member);

        if (IsOwner)
            return true;

        if (member.IsOwner)
            return false;

        if (Guild.Id != member.Guild.Id)
            return false;

        var myHighestRole = Roles.FirstOrDefault();
        var theirHighestRole = member.Roles.FirstOrDefault();

        if (myHighestRole == null)
            return false;

        if (theirHighestRole == null)
            return true;

        return myHighestRole.CanInteract(theirHighestRole);
    }

    /// <inheritdoc />
    public bool CanInteract(IRole role)
    {
        ArgumentNullException.ThrowIfNull(role);

        if (IsOwner)
            return true;

        if (Guild.Id != role.Guild.Id)
            return false;

        var myHighestRole = Roles.FirstOrDefault();
        if (myHighestRole == null)
            return false;

        return myHighestRole.CanInteract(role);
    }

    /// <inheritdoc />
    public bool CanInteract(Emoji emoji)
    {
        if (emoji == null)
            throw new ArgumentNullException(nameof(emoji));


        return true;
    }

    /// <inheritdoc />
    public Task BanAsync(int deletionTimeframe, CancellationToken cancellationToken = default)
    {
        return _guild.Bans.Ban(Id, deletionTimeframe).ExecuteAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task KickAsync(CancellationToken cancellationToken = default)
    {
        return _guild.Members.Kick(Id).ExecuteAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task TimeoutForAsync(TimeSpan duration, CancellationToken cancellationToken = default)
    {
        return TimeoutUntilAsync(DateTimeOffset.UtcNow + duration, cancellationToken);
    }

    /// <inheritdoc />
    public Task TimeoutUntilAsync(DateTimeOffset until, CancellationToken cancellationToken = default)
    {

        throw new NotImplementedException("TimeoutUntilAsync is not yet implemented.");
    }

    /// <inheritdoc />
    public Task RemoveTimeoutAsync(CancellationToken cancellationToken = default)
    {
        return TimeoutUntilAsync(DateTimeOffset.MinValue, cancellationToken);
    }

    /// <inheritdoc />
    public Task MuteAsync(bool mute, CancellationToken cancellationToken = default)
    {

        throw new NotImplementedException("MuteAsync is not yet implemented.");
    }

    /// <inheritdoc />
    public Task DeafenAsync(bool deafen, CancellationToken cancellationToken = default)
    {

        throw new NotImplementedException("DeafenAsync is not yet implemented.");
    }

    /// <inheritdoc />
    public Task ModifyNicknameAsync(string? nickname, CancellationToken cancellationToken = default)
    {

        throw new NotImplementedException("ModifyNicknameAsync is not yet implemented.");
    }

    /// <inheritdoc />
    public Task ModifyFlagsAsync(IEnumerable<MemberFlag> newFlags, CancellationToken cancellationToken = default)
    {

        throw new NotImplementedException("ModifyFlagsAsync is not yet implemented.");
    }
}
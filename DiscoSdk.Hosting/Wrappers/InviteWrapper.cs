using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Wrapper that implements <see cref="IInvite"/> for a <see cref="Invite"/> instance.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="InviteWrapper"/> class.
/// </remarks>
/// <param name="invite">The invite instance to wrap.</param>
/// <param name="client">The Discord client for performing operations.</param>
internal class InviteWrapper(Invite invite, IGuildChannel channel, DiscordClient client) : IInvite
{
    private readonly Invite _invite = invite ?? throw new ArgumentNullException(nameof(invite));
    private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));

    /// <inheritdoc />
    public string Code => _invite.Code;

    /// <inheritdoc />
    public IChannel Channel => channel;

    /// <inheritdoc />
    public IGuild Guild => channel.Guild;

    /// <inheritdoc />
    public IUser? Inviter { get; } = invite.Inviter != null ? new UserWrapper(invite.Inviter) : null;

    /// <inheritdoc />
    public int? TargetType => _invite.TargetType;

    /// <inheritdoc />
    public IUser? TargetUser { get; } = invite.TargetUser != null ? new UserWrapper(invite.TargetUser) : null;

    /// <inheritdoc />
    public DiscordId? TargetApplicationId => _invite.TargetApplicationId;

    /// <inheritdoc />
    public int? ApproximatePresenceCount => _invite.ApproximatePresenceCount;

    /// <inheritdoc />
    public int? ApproximateMemberCount => _invite.ApproximateMemberCount;

    /// <inheritdoc />
    public int? Uses => _invite.Uses;

    /// <inheritdoc />
    public int? MaxUses => _invite.MaxUses;

    /// <inheritdoc />
    public int? MaxAge => _invite.MaxAge;

    /// <inheritdoc />
    public bool? Temporary => _invite.Temporary;

    /// <inheritdoc />
    public DateTimeOffset? CreatedAt => _invite.CreatedAt != null
        ? DateTimeOffset.Parse(_invite.CreatedAt)
        : null;

    /// <inheritdoc />
    public DateTimeOffset? ExpiresAt => _invite.ExpiresAt != null
        ? DateTimeOffset.Parse(_invite.ExpiresAt)
        : null;

    /// <inheritdoc />
    public bool IsExpired
    {
        get
        {
            if (ExpiresAt == null)
                return false;

            return DateTimeOffset.UtcNow >= ExpiresAt.Value;
        }
    }

    /// <inheritdoc />
    public string Url => $"https://discord.gg/{Code}";

    /// <inheritdoc />
    public bool IsMaxedOut
    {
        get
        {
            if (MaxUses == null)
                return false;

            if (Uses == null)
                return false;

            return Uses >= MaxUses;
        }
    }

    /// <inheritdoc />
    public IRestAction Delete()
    {
        return new RestAction(cancellationToken => _client.InviteClient.DeleteAsync(Code, cancellationToken));
    }
}
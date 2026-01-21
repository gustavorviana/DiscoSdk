using DiscoSdk.Models.Channels;

namespace DiscoSdk.Models.Messages.Mentions;

/// <summary>
/// Represents a typed mention inside a message, encapsulating both its textual
/// representation and its notification behavior.
///
/// A <see cref="Mention"/> models the Discord mention system at a semantic level:
/// it knows *what* is being mentioned (user, role, everyone, channel) and whether
/// that mention should actually trigger a notification (ping).
///
/// This abstraction allows higher-level builders to generate both the message
/// content and the corresponding <c>allowed_mentions</c> payload safely,
/// preventing accidental mass pings.
/// </summary>
public readonly struct Mention : IEquatable<Mention>
{
    /// <summary>
    /// Gets the kind of mention (User, Role, Everyone, Channel).
    /// </summary>
    public MentionType Type { get; }

    /// <summary>
    /// Gets the referenced entity ID, when applicable.
    /// For <see cref="MentionType.Everyone"/> this value is always <c>null</c>.
    /// </summary>
    public Snowflake? Id { get; }

    /// <summary>
    /// Gets whether this mention should trigger a notification.
    /// When <c>false</c>, the mention is rendered visually but does not ping.
    /// </summary>
    public bool Ping { get; }

    /// <summary>
    /// Creates a new <see cref="Mention"/> instance.
    /// Validation enforces Discord rules, such as:
    /// - <see cref="MentionType.Everyone"/> cannot have an ID.
    /// - User/Role/Channel mentions must have an ID.
    /// - Channel mentions can never notify.
    /// </summary>
    public Mention(MentionType type, Snowflake? id, bool ping)
    {
        ValidateMention(type, id, ping);

        Type = type;
        Id = id;
        Ping = ping;
    }

    /// <summary>
    /// Validates mention invariants according to Discord semantics.
    /// </summary>
    private static void ValidateMention(MentionType type, Snowflake? id, bool ping)
    {
        if (type == MentionType.Everyone)
        {
            if (id != null)
                throw new InvalidOperationException("Everyone mention must not have an ID.");

            return;
        }

        // All other mention types require an ID
        if (id == null)
            throw new InvalidOperationException($"Mention type '{type}' requires an ID.");

        // Channels can never notify; they are always silent links
        if (type == MentionType.Channel && ping)
            throw new InvalidOperationException("Channel mentions cannot produce notifications.");
    }

    /// <summary>
    /// Creates an <c>@everyone</c> mention.
    /// </summary>
    public static Mention Everyone(bool ping)
        => new(MentionType.Everyone, null, ping);

    /// <summary>
    /// Creates a role mention.
    /// </summary>
    public static Mention FromRole(IRole role, bool ping)
        => new(MentionType.Role, role.Id, ping);

    /// <summary>
    /// Creates a user mention.
    /// </summary>
    public static Mention FromUser(IUser user, bool ping)
        => new(MentionType.User, user.Id, ping);

    /// <summary>
    /// Creates a channel mention from a channel instance.
    /// Channel mentions are always silent.
    /// </summary>
    public static Mention FromChannel(IChannel channel)
        => FromChannel(channel.Id);

    /// <summary>
    /// Creates a channel mention from a channel ID.
    /// Channel mentions are always silent.
    /// </summary>
    public static Mention FromChannel(Snowflake id)
        => new(MentionType.Channel, id, false);

    public override bool Equals(object? obj)
    {
        return obj is Mention mention && Equals(mention);
    }

    public bool Equals(Mention other)
    {
        return Type == other.Type &&
               EqualityComparer<Snowflake?>.Default.Equals(Id, other.Id) &&
               Ping == other.Ping;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Type, Id, Ping);
    }

    public static bool operator ==(Mention left, Mention right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Mention left, Mention right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Returns the Discord textual representation of this mention.
    /// </summary>
    public override string ToString()
    {
        return Type switch
        {
            MentionType.User => $"<@{Id}>",
            MentionType.Role => $"<@&{Id}>",
            _ => "@everyone",
        };
    }
}

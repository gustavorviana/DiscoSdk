using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Messages;

/// <summary>
/// Represents a user mention that appeared in a received Discord message.
/// This is a message-scoped view of the mentioned user, and can be used to
/// access identifiers, display info, and to resolve richer user/member data
/// when needed.
/// </summary>
public interface IUserMention
{
    /// <summary>
    /// Gets the mentioned user's ID.
    /// </summary>
    Snowflake UserId { get; }

    IGuild? Guild { get; }

    /// <summary>
    /// Gets the username as provided by the message payload.
    /// </summary>
    string Username { get; }

    /// <summary>
    /// Gets the global display name as provided by the message payload, if any.
    /// </summary>
    string? GlobalName { get; }

    /// <summary>
    /// Gets the avatar hash as provided by the message payload, if any.
    /// </summary>
    DiscordImageUrl? Avatar { get; }

    /// <summary>
    /// Gets the banner hash as provided by the message payload, if any.
    /// </summary>
    DiscordImageUrl? Banner { get; }

    /// <summary>
    /// Gets the raw bitfield of public badges/attributes as provided by the message payload.
    /// </summary>
    UserFlags PublicFlags { get; }

    /// <summary>
    /// Gets the raw bitfield of contextual flags as provided by the message payload.
    /// </summary>
    UserFlags Flags { get; }

    /// <summary>
    /// Resolves the full user object via REST (or cache+REST), if available in the current client context.
    /// </summary>
    IRestAction<IUser> ResolveUserAsync();

    /// <summary>
    /// Resolves the guild member representation for this user when the message context is a guild.
    /// Returns null if the message is not in a guild or the member is not accessible.
    /// </summary>
    IRestAction<IMember?> ResolveMemberAsync();
}
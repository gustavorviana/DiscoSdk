using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages.Embeds;
using DiscoSdk.Models.Messages.Pools;

namespace DiscoSdk.Models.Messages;

public interface IMessageBase
{
    /// <summary>
    /// Gets the type of message.
    /// </summary>
    MessageType Type { get; }

    Snowflake[] MentionRoles { get; }

    /// <summary>
    /// Gets the contents of the message.
    /// </summary>
    string Content { get; }

    /// <summary>
    /// Gets any embedded content.
    /// </summary>
    Embed[] Embeds { get; }

    /// <summary>
    /// Gets any attached files.
    /// </summary>
    Attachment[] Attachments { get; }

    /// <summary>
    /// Gets when this message was edited.
    /// </summary>
    DateTimeOffset? UpdatedAt { get; }

    /// <summary>
    /// Gets a value indicating whether this message is pinned.
    /// </summary>
    bool Pinned { get; }

    /// <summary>
    /// Gets the message flags.
    /// </summary>
    MessageFlags Flags { get; }

    /// <summary>
    /// Gets the pool of message.
    /// </summary>
    Poll? Poll { get; }
}
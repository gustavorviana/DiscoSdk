using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Embeds;
using DiscoSdk.Models.Messages.Pools;
using System.Globalization;

namespace DiscoSdk.Hosting.Wrappers.Messages;

internal abstract class MessageBaseWrapper(Message message) : IMessageBase
{
    protected Message Message => message ?? throw new ArgumentNullException(nameof(message));

    public virtual string Content => message.Content;

    public virtual Poll? Poll => message.Pool;

    public virtual Embed[] Embeds => message.Embeds;

    public virtual Attachment[] Attachments => message.Attachments;

    public DateTimeOffset? UpdatedAt { get; }
        = message.EditedTimestamp != null
        ? DateTimeOffset.Parse(message.EditedTimestamp, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
        : null;

    public bool Pinned => message.Pinned;

    public MessageFlags Flags => message.Flags;

    public MessageType Type => message.Type;

    public Snowflake[] MentionRoles { get; }
        = [.. message
        .MentionRoles
        .Select(x => Snowflake.TryParse(x, out var role) ? role : default)
        .Where(x => x != default)];
}
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Models.Messages.Embeds;

namespace DiscoSdk.Hosting.Wrappers.Messages;

internal sealed class MessageSnapshotWrapper(DiscordClient? client, MessageSnapshot model, IGuild? guild) : IMessageSnapshot
{
	private IReadOnlyList<IUserMention>? _mentions;
	private IReadOnlyList<Snowflake>? _mentionRoles;

	private MessageSnapshotPayload Payload => model.Message;

	public MessageType Type => Payload.Type;
	public string? Content => Payload.Content;
	public IReadOnlyList<Embed> Embeds => Payload.Embeds;
	public IReadOnlyList<Attachment> Attachments => Payload.Attachments;
	public string Timestamp => Payload.Timestamp;
	public string? EditedTimestamp => Payload.EditedTimestamp;
	public MessageFlags Flags => Payload.Flags;

	public IReadOnlyList<IUserMention> Mentions => _mentions ??=
		[.. Payload.Mentions.Select(m => new UserMentionWrapper(client, m, guild))];

	public IReadOnlyList<Snowflake> MentionRoles => _mentionRoles ??=
		[.. Payload.MentionRoles.Select(Snowflake.Parse)];

	public IReadOnlyList<StickerItem>? StickerItems => Payload.StickerItems;
	public IReadOnlyList<IInteractionComponent>? Components => Payload.Components;
}

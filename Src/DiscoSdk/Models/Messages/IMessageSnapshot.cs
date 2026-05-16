using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Models.Messages.Embeds;

namespace DiscoSdk.Models.Messages;

/// <summary>
/// Read-only view of a frozen <see cref="IMessage"/> embedded inside a forwarded message via
/// <see cref="IMessage.MessageSnapshots"/>. Discord populates the snapshot at forward-time;
/// later edits or deletes of the source message do <strong>not</strong> propagate here.
/// </summary>
/// <remarks>
/// Source: <see href="https://discord.com/developers/docs/resources/message#message-snapshot-object"/>.
/// Snapshots intentionally omit the original message's identity (id/channel_id/author/guild) —
/// only the curated content fields are present.
/// </remarks>
public interface IMessageSnapshot
{
	/// <summary>Source message type at the moment of forwarding.</summary>
	MessageType Type { get; }

	/// <summary>Frozen text content of the source message.</summary>
	string? Content { get; }

	/// <summary>Frozen embeds of the source message.</summary>
	IReadOnlyList<Embed> Embeds { get; }

	/// <summary>Frozen attachments of the source message.</summary>
	IReadOnlyList<Attachment> Attachments { get; }

	/// <summary>When the source message was originally sent.</summary>
	string Timestamp { get; }

	/// <summary>When the source message was last edited prior to the forward, if any.</summary>
	string? EditedTimestamp { get; }

	/// <summary>Flags of the source message at the moment of forwarding.</summary>
	MessageFlags Flags { get; }

	/// <summary>Users mentioned by the source message.</summary>
	IReadOnlyList<IUserMention> Mentions { get; }

	/// <summary>Role IDs mentioned by the source message.</summary>
	IReadOnlyList<Snowflake> MentionRoles { get; }

	/// <summary>Frozen sticker items of the source message, or <c>null</c> if none.</summary>
	IReadOnlyList<StickerItem>? StickerItems { get; }

	/// <summary>Frozen components of the source message (V1 action rows or V2 components).</summary>
	IReadOnlyList<IInteractionComponent>? Components { get; }
}

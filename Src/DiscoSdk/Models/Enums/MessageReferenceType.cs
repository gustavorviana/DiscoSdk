namespace DiscoSdk.Models.Enums;

/// <summary>
/// Kind of relationship between a message and the message it references via
/// <see cref="Messages.MessageReference"/>. Maps directly to Discord's
/// <c>message_reference.type</c> wire value.
/// </summary>
/// <remarks>
/// Source: <see href="https://discord.com/developers/docs/resources/message#message-reference-types"/>.
/// </remarks>
public enum MessageReferenceType
{
	/// <summary>
	/// Standard reply / crosspost / channel-follow / pin reference. The referenced message is
	/// surfaced inline via <c>referenced_message</c>. This is the default and matches the
	/// historical (pre-FORWARD) behaviour.
	/// </summary>
	Default = 0,

	/// <summary>
	/// Forward reference. The receiving message has no original content of its own; instead
	/// Discord populates <c>message_snapshots</c> with a frozen subset of the source message.
	/// Forwards do not ping anyone and the bot's own message stays empty (no content / embeds).
	/// </summary>
	Forward = 1,
}

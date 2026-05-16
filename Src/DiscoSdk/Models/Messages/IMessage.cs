using DiscoSdk.Exceptions;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Models.Users;
using DiscoSdk.Rest.Actions;
using DiscoSdk.Rest.Actions.Messages;

namespace DiscoSdk.Models.Messages;

/// <summary>
/// Represents a Discord message with visual properties and operations.
/// </summary>
/// <remarks>
/// When this message is backed by a Webhook:
/// - No read operations are available via the Webhook API.
/// - Only messages created by the same Webhook can be edited or deleted.
/// - Replying, reacting, pinning, crossposting, and querying reactions are NOT supported.
/// - Components are visual-only; they do not produce interaction events.
/// </remarks>
public interface IMessage : IMessageBase, IMentionable, IDeletable
{
    /// <summary>
    /// Gets the channel the message was sent in.
    /// </summary>
    ITextBasedChannel Channel { get; }

    /// <summary>
    /// Gets the ID of the guild the message was sent in.
    /// </summary>
    IGuild? Guild { get; }

    /// <summary>
    /// Gets the author of the message.
    /// </summary>
    /// <remarks>
    /// Webhook messages do not have a real user identity.
    /// </remarks>
    IUser Author { get; }

    /// <summary>
    /// Gets the components of the message. Concrete entries are either V1 components
    /// (<see cref="MessageComponent"/> action rows) or V2 components
    /// (<see cref="SectionComponent"/>, <see cref="ContainerComponent"/>, etc.).
    /// </summary>
    /// <remarks>
    /// In Webhook messages, components are visual-only and do not generate interactions.
    /// </remarks>
    IInteractionComponent[]? Components { get; }

    /// <summary>
    /// Gets the reactions to the message.
    /// </summary>
    /// <remarks>
    /// Webhook API cannot query reactions.
    /// </remarks>
    IReaction[] Reactions { get; }

    /// <summary>
    /// Frozen snapshots of source messages when this message was created as a forward. Empty
    /// array for non-forward messages.
    /// </summary>
    /// <remarks>
    /// Snapshots do not propagate edits/deletes from the source — they are point-in-time
    /// copies. The original message is identified by <c>message_reference</c> (with
    /// <c>type = Forward</c>) on the same message object.
    /// </remarks>
    IReadOnlyList<IMessageSnapshot> MessageSnapshots { get; }

    IUserMention[] Mentions { get; }

    /// <summary>
    /// Creates a builder for editing this message.
    /// </summary>
    /// <returns>An <see cref="IEditMessageRestAction"/> instance for editing the message.</returns>
    /// <remarks>
    /// Webhook can only edit messages created by the same Webhook and requires the message id to be known.
    /// </remarks>
    IEditMessageRestAction Edit();

    /// <summary>
    /// Creates a builder for replying to this message.
    /// </summary>
    /// <param name="content">The initial reply content.</param>
    /// <returns>An <see cref="ISendMessageRestAction"/> instance for sending the reply.</returns>
    /// <remarks>
    /// Not supported for Webhooks.
    /// </remarks>
    ISendMessageRestAction Reply(string? content = null);

    /// <summary>
    /// Creates a builder for forwarding this message to <paramref name="target"/>. The returned
    /// action is pre-configured with the Forward-type message reference pointing back to this
    /// message; calling <c>ExecuteAsync</c> produces a frozen <see cref="MessageSnapshot"/> of
    /// this message inside <paramref name="target"/>.
    /// </summary>
    /// <param name="target">The channel that will receive the forward.</param>
    /// <returns>An <see cref="ISendMessageRestAction"/> targeting <paramref name="target"/>.</returns>
    /// <remarks>
    /// Forwards carry no content/embeds/components of their own — Discord ignores those fields
    /// when <c>message_reference.type</c> is <c>Forward</c>. The snapshot stays frozen even if
    /// the source message is later edited or deleted.
    /// <para>
    /// Docs: <see href="https://discord.com/developers/docs/resources/message#message-reference-types"/>.
    /// </para>
    /// </remarks>
    ISendMessageRestAction ForwardTo(Channels.ITextBasedChannel target);

    /// <summary>
    /// Crossposts this message (only for news channels).
    /// </summary>
    /// <returns>A REST action that can be executed to crosspost the message.</returns>
    /// <remarks>
    /// Not supported for Webhooks.
    /// </remarks>
    IRestAction<IMessage> Crosspost();

    /// <summary>
    /// Adds a reaction to this message.
    /// </summary>
    /// <param name="emoji">The emoji to react with (URL-encoded if custom emoji).</param>
    /// <returns>A REST action that can be executed to add the reaction.</returns>
    /// <remarks>
    /// Requires <see cref="DiscordIntent.GuildMessageReactions"/> for guild messages or
    /// <see cref="DiscordIntent.DirectMessageReactions"/> for DMs.
    /// Not supported for Webhooks.
    /// </remarks>
    /// <exception cref="DiscoSdk.Exceptions.MissingIntentException">
    /// Thrown when the appropriate reactions intent is not enabled on the client.
    /// </exception>
    IRestAction AddReaction(string emoji);

    /// <summary>
    /// Gets all users who reacted with a specific emoji to this message.
    /// </summary>
    /// <param name="emoji">The emoji to get reactions for (URL-encoded if custom emoji).</param>
    /// <param name="after">Get users after this user ID.</param>
    /// <param name="limit">Maximum number of users to return (1-100, default 25).</param>
    /// <returns>A REST action that can be executed to get the reactions.</returns>
    /// <remarks>
    /// Requires <see cref="DiscordIntent.GuildMessageReactions"/> for guild messages or
    /// <see cref="DiscordIntent.DirectMessageReactions"/> for DMs.
    /// Not supported for Webhooks.
    /// </remarks>
    /// <exception cref="DiscoSdk.Exceptions.MissingIntentException">
    /// Thrown when the appropriate reactions intent is not enabled on the client.
    /// </exception>
    IGetReactionsAction GetReactions(string emoji);

    /// <summary>
    /// Deletes all reactions of a specific emoji from this message.
    /// </summary>
    /// <param name="emoji">The emoji to remove all reactions for (URL-encoded if custom emoji).</param>
    /// <returns>A REST action that can be executed to delete all reactions for the emoji.</returns>
    /// <remarks>
    /// Requires <see cref="DiscordIntent.GuildMessageReactions"/> for guild messages or
    /// <see cref="DiscordIntent.DirectMessageReactions"/> for DMs.
    /// Not supported for Webhooks.
    /// </remarks>
    /// <exception cref="DiscoSdk.Exceptions.MissingIntentException">
    /// Thrown when the appropriate reactions intent is not enabled on the client.
    /// </exception>
    IRestAction DeleteAllReactionsForEmoji(string emoji);

    /// <summary>
    /// Deletes all reactions from this message.
    /// </summary>
    /// <returns>A REST action that can be executed to delete all reactions.</returns>
    /// <remarks>
    /// Requires <see cref="DiscordIntent.GuildMessageReactions"/> for guild messages or
    /// <see cref="DiscordIntent.DirectMessageReactions"/> for DMs.
    /// Not supported for Webhooks.
    /// </remarks>
    /// <exception cref="DiscoSdk.Exceptions.MissingIntentException">
    /// Thrown when the appropriate reactions intent is not enabled on the client.
    /// </exception>
    IRestAction DeleteAllReactions();

    /// <summary>
    /// Pins this message in the channel.
    /// </summary>
    /// <returns>A REST action that can be executed to pin the message.</returns>
    /// <remarks>
    /// Not supported for Webhooks.
    /// </remarks>
    /// <exception cref="EphemeralMessageException">Thrown when attempting to pin an ephemeral message.</exception>
    IRestAction Pin();

    /// <summary>
    /// Unpins this message from the channel.
    /// </summary>
    /// <returns>A REST action that can be executed to unpin the message.</returns>
    /// <remarks>
    /// Not supported for Webhooks.
    /// </remarks>
    /// <exception cref="EphemeralMessageException">Thrown when attempting to unpin an ephemeral message.</exception>
    IRestAction Unpin();
}
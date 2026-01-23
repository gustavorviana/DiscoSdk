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
    /// Gets the components of the message.
    /// </summary>
    /// <remarks>
    /// In Webhook messages, components are visual-only and do not generate interactions.
    /// </remarks>
    MessageComponent[]? Components { get; }

    /// <summary>
    /// Gets the reactions to the message.
    /// </summary>
    /// <remarks>
    /// Webhook API cannot query reactions.
    /// </remarks>
    IReaction[] Reactions { get; }

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
    /// Not supported for Webhooks.
    /// </remarks>
    IRestAction AddReaction(string emoji);

    /// <summary>
    /// Gets all users who reacted with a specific emoji to this message.
    /// </summary>
    /// <param name="emoji">The emoji to get reactions for (URL-encoded if custom emoji).</param>
    /// <param name="after">Get users after this user ID.</param>
    /// <param name="limit">Maximum number of users to return (1-100, default 25).</param>
    /// <returns>A REST action that can be executed to get the reactions.</returns>
    /// <remarks>
    /// Not supported for Webhooks.
    /// </remarks>
    IRestAction<User[]> GetReactions(string emoji, string? after = null, int? limit = null);

    /// <summary>
    /// Deletes all reactions of a specific emoji from this message.
    /// </summary>
    /// <param name="emoji">The emoji to remove all reactions for (URL-encoded if custom emoji).</param>
    /// <returns>A REST action that can be executed to delete all reactions for the emoji.</returns>
    /// <remarks>
    /// Not supported for Webhooks.
    /// </remarks>
    IRestAction DeleteAllReactionsForEmoji(string emoji);

    /// <summary>
    /// Deletes all reactions from this message.
    /// </summary>
    /// <returns>A REST action that can be executed to delete all reactions.</returns>
    /// <remarks>
    /// Not supported for Webhooks.
    /// </remarks>
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
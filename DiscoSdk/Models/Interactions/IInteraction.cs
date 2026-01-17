using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Interactions;

/// <summary>
/// Represents a Discord interaction with all its properties and available actions.
/// </summary>
/// <remarks>
/// All Discord IDs must be of type <see cref="Snowflake"/>.
/// All methods that perform server actions return <see cref="IRestAction"/> or <see cref="IRestAction{T}"/>.
/// </remarks>
public interface IInteraction
{
	/// <summary>
	/// Gets the ID of the interaction.
	/// </summary>
	Snowflake Id { get; }

	/// <summary>
	/// Gets the application ID.
	/// </summary>
	Snowflake ApplicationId { get; }

	/// <summary>
	/// Gets the type of interaction.
	/// </summary>
	InteractionType Type { get; }

	/// <summary>
	/// Gets the interaction data, if the interaction is of type APPLICATION_COMMAND, MESSAGE_COMPONENT, or MODAL_SUBMIT.
	/// </summary>
	IInteractionData? Data { get; }

	/// <summary>
	/// Gets the guild where the interaction was triggered, or null if triggered in a DM.
	/// </summary>
	IGuild? Guild { get; }

    /// <summary>
    /// Gets the channel ID where the interaction was triggered.
    /// </summary>
    ITextBasedChannel? Channel { get; }

	/// <summary>
	/// Gets the member who triggered the interaction, or null if triggered in a DM.
	/// </summary>
	IMember? Member { get; }

	/// <summary>
	/// Gets the user who triggered the interaction.
	/// </summary>
	IUser? User { get; }

	/// <summary>
	/// Gets the continuation token for responding to the interaction.
	/// </summary>
	string Token { get; }

	/// <summary>
	/// Gets the read-only version property, always 1.
	/// </summary>
	int Version { get; }

	/// <summary>
	/// Gets the message this interaction was attached to (for message component interactions).
	/// </summary>
	IMessage? Message { get; }

	/// <summary>
	/// Gets the selected language of the invoking user.
	/// </summary>
	string? Locale { get; }

	/// <summary>
	/// Gets the guild's preferred locale, or null if triggered in a DM.
	/// </summary>
	string? GuildLocale { get; }

	/// <summary>
	/// Defers the interaction response, indicating that the response will be sent later.
	/// </summary>
	/// <param name="ephemeral">Whether the deferred response should be ephemeral (only visible to the user who triggered the interaction).</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	/// <remarks>
	/// After deferring, you can use <see cref="FollowUp"/> to send follow-up messages.
	/// </remarks>
	IRestAction Defer(bool ephemeral = false);

	/// <summary>
	/// Creates a REST action to reply to this interaction with a message.
	/// </summary>
	/// <param name="content">The initial message content.</param>
	/// <returns>A REST action that can be configured and executed to send the reply.</returns>
	/// <remarks>
	/// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
	/// </remarks>
	ISendMessageRestAction Reply(string? content = null);

	/// <summary>
	/// Creates a REST action to reply to this interaction with a modal dialog.
	/// </summary>
	/// <returns>A REST action that can be configured and executed to reply with the modal.</returns>
	/// <remarks>
	/// The action is not executed immediately. Call <see cref="IRestAction.ExecuteAsync"/> to execute it.
	/// Use <see cref="IReplyModalRestAction.SetCustomId"/>, <see cref="IReplyModalRestAction.SetTitle"/>, and
	/// <see cref="IReplyModalRestAction.AddActionRow"/> to configure the modal before executing.
	/// </remarks>
	/// <exception cref="InvalidOperationException">Thrown if the interaction has already been deferred or responded to.</exception>
	IReplyModalRestAction ReplyModal();

	/// <summary>
	/// Creates a REST action to edit the original interaction response message.
	/// </summary>
	/// <returns>A REST action that can be configured and executed to edit the original response.</returns>
	/// <remarks>
	/// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
	/// This can only be used after the interaction has been responded to.
	/// </remarks>
	IEditMessageRestAction Edit();

	/// <summary>
	/// Creates a REST action to send a follow-up message to this interaction.
	/// </summary>
	/// <param name="content">The initial message content.</param>
	/// <returns>A REST action that can be configured and executed to send the follow-up message.</returns>
	/// <remarks>
	/// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
	/// Follow-up messages can be sent after deferring or responding to the interaction.
	/// </remarks>
	ISendMessageRestAction FollowUp(string? content = null);

	/// <summary>
	/// Gets a REST action for deleting the original interaction response message.
	/// </summary>
	/// <returns>A REST action that can be executed to delete the original response.</returns>
	/// <remarks>
	/// The action is not executed immediately. Call <see cref="IRestAction.ExecuteAsync"/> to execute it.
	/// </remarks>
	IRestAction Delete();
}
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Components;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a REST action for replying to an interaction with a modal.
/// </summary>
public interface IReplyModalRestAction : IRestAction
{
	/// <summary>
	/// Sets the custom ID for the modal (max 100 characters).
	/// </summary>
	/// <param name="customId">The custom ID for the modal.</param>
	/// <returns>The current <see cref="IReplyModalRestAction"/> instance.</returns>
	IReplyModalRestAction SetCustomId(string customId);

	/// <summary>
	/// Sets the title of the modal (max 45 characters).
	/// </summary>
	/// <param name="title">The title of the modal.</param>
	/// <returns>The current <see cref="IReplyModalRestAction"/> instance.</returns>
	IReplyModalRestAction SetTitle(string title);

	/// <summary>
	/// Adds an action row containing a text input component to the modal.
	/// </summary>
	/// <param name="textInput">The text input component to add.</param>
	/// <returns>The current <see cref="IReplyModalRestAction"/> instance.</returns>
	IReplyModalRestAction AddActionRow(IModalComponent textInput);

	/// <summary>
	/// Adds an action row containing a modal component built from an <see cref="IModalComponentBuilder"/> (e.g. <see cref="TextInputBuilder"/>).
	/// </summary>
	/// <param name="builder">The modal component builder to build and add.</param>
	/// <returns>The current <see cref="IReplyModalRestAction"/> instance.</returns>
	/// <exception cref="ArgumentNullException">Thrown when builder is null.</exception>
	IReplyModalRestAction AddActionRow(IModalComponentBuilder builder);

	/// <summary>
	/// Sets the components of the modal (action rows only; each row contains components that have their own label property).
	/// </summary>
	/// <param name="components">The modal components to set.</param>
	/// <returns>The current <see cref="IReplyModalRestAction"/> instance.</returns>
	IReplyModalRestAction SetComponents(params IModalComponent[] components);
}


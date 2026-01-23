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
	IReplyModalRestAction AddActionRow(TextInputComponent textInput);

	/// <summary>
	/// Sets the components (action rows) of the modal.
	/// </summary>
	/// <param name="components">The action row components to set.</param>
	/// <returns>The current <see cref="IReplyModalRestAction"/> instance.</returns>
	IReplyModalRestAction SetComponents(params ActionRowComponent[] components);
}


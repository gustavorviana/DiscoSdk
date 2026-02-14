using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Interactions;

/// <summary>
/// Represents interaction data for application commands, message components, and modals.
/// </summary>
/// <remarks>
/// All Discord IDs must be of type <see cref="Snowflake"/>.
/// </remarks>
public interface IInteractionData
{
	/// <summary>
	/// Gets the ID of the invoked command (for application commands).
	/// </summary>
	Snowflake? Id { get; }

	/// <summary>
	/// Gets the name of the invoked command (for application commands).
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Gets the type of the invoked command (for application commands).
	/// </summary>
	ApplicationCommandType Type { get; }

	/// <summary>
	/// Gets the resolved data containing users, members, roles, channels, and messages referenced in the interaction.
	/// </summary>
	IRestAction<IInteractionResolved?> GetResolved();

	/// <summary>
	/// Gets the parameters and values from the user (for application commands).
	/// </summary>
	InteractionOption[]? Options { get; }

	/// <summary>
	/// Gets the custom ID of the component (for message components and modals).
	/// </summary>
	string? CustomId { get; }

	/// <summary>
	/// Gets the type of component (for message components).
	/// </summary>
	ComponentType? ComponentType { get; }

	/// <summary>
	/// Gets the values the user selected (for select menu components).
	/// </summary>
	string[]? Values { get; }

	/// <summary>
	/// Gets the rows of submitted fields when this interaction is MODAL_SUBMIT.
	/// </summary>
	ModalSubmitRow[]? Components { get; }
}


using DiscoSdk.Models.Commands;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Contexts.Interactions;

/// <summary>
/// Context provided to autocomplete handlers when the user is typing in an option with autocomplete enabled.
/// </summary>
public interface IAutocompleteContext : IInteractionContext
{
	/// <summary>
	/// Gets the name of the command being autocompleted.
	/// </summary>
	string CommandName { get; }

	/// <summary>
	/// Gets the subcommand name when the command has subcommands; otherwise null.
	/// </summary>
	string? Subcommand { get; }

	/// <summary>
	/// Gets the option that is currently focused (the field being autocompleted).
	/// </summary>
	IAutocompleteFocusedOption FocusedOption { get; }

	/// <summary>
	/// Gets the other options already filled by the user, for context when generating suggestions.
	/// </summary>
	IReadOnlyCollection<IAutocompleteOptionValue> Options { get; }

	/// <summary>
	/// Returns an action that responds to the autocomplete interaction with the given choices (max 25).
	/// </summary>
	/// <param name="choices">The suggestions to display. Maximum 25 choices; can be empty.</param>
	/// <returns>An <see cref="IRestAction"/> that sends the autocomplete response when executed.</returns>
	IRestAction ReplyWithChoices(IEnumerable<ApplicationCommandOptionChoice> choices);
}
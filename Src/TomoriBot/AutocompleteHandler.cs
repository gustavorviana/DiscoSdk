using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Events;
using DiscoSdk.Models.Commands;

namespace TomoriBot;

/// <summary>
/// Handles autocomplete for application commands. Only called when the user is typing in an option with autocomplete enabled.
/// </summary>
internal class AutocompleteHandler : IAutocompleteHandler
{
	private static readonly string[] FruitSuggestions =
	[
		"Apple", "Apricot", "Avocado", "Banana", "Blackberry", "Blueberry", "Cherry",
		"Coconut", "Grape", "Kiwi", "Lemon", "Mango", "Melon", "Orange", "Peach",
		"Pear", "Pineapple", "Plum", "Raspberry", "Strawberry", "Watermelon"
	];

	public async Task HandleAsync(IAutocompleteContext context)
	{
		// Example: autocomplete for "search" command, option "query"
		if (context.CommandName != "search")
			return;

		var focused = context.FocusedOption;
		if (focused.Name != "query")
			return;

		var partial = (focused.Value as string)?.Trim() ?? "";
		var filtered = FruitSuggestions
			.Where(s => s.Contains(partial, StringComparison.OrdinalIgnoreCase))
			.Take(25)
			.Select(s => new ApplicationCommandOptionChoice { Name = s, Value = s })
			.ToList();

		await context.ReplyWithChoices(filtered).ExecuteAsync();
	}
}

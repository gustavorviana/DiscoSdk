using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Events;
using DiscoSdk.Models.Commands;

namespace TomoriBot;

/// <summary>
/// Handles AutoComplete for application commands. Only called when the user is typing in an option with AutoComplete enabled.
/// </summary>
internal class AutoCompleteHandler : IAutoCompleteHandler
{
	private static readonly string[] FruitSuggestions =
	[
		"Apple", "Apricot", "Avocado", "Banana", "Blackberry", "Blueberry", "Cherry",
		"Coconut", "Grape", "Kiwi", "Lemon", "Mango", "Melon", "Orange", "Peach",
		"Pear", "Pineapple", "Plum", "Raspberry", "Strawberry", "Watermelon"
	];

	public async Task HandleAsync(IAutoCompleteContext context, IServiceProvider services)
	{
		// Example: AutoComplete for "search" command, option "query"
		if (context.CommandName != "search")
			return;

		var focused = context.FocusedOption;
		if (focused.Name != "query")
			return;

		var partial = (focused.Value as string)?.Trim() ?? "";
		var filtered = FruitSuggestions
			.Where(s => s.Contains(partial, StringComparison.OrdinalIgnoreCase))
			.Take(25)
			.Select(s => new SlashCommandOptionChoice { Name = s, Value = s })
			.ToList();

		await context.ReplyWithChoices(filtered).ExecuteAsync();
	}
}

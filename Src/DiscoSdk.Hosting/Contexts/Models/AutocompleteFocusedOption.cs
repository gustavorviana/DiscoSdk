using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Hosting.Contexts.Models;

internal class AutocompleteFocusedOption(string name, SlashCommandOptionType type, object? value) : IAutocompleteFocusedOption
{
	public string Name => name;
	public SlashCommandOptionType Type => type;
	public object? Value => value;
}

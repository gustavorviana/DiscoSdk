using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Hosting.Contexts.Models;

internal class AutoCompleteFocusedOption(string name, SlashCommandOptionType type, object? value) : IAutoCompleteFocusedOption
{
	public string Name => name;
	public SlashCommandOptionType Type => type;
	public object? Value => value;
}

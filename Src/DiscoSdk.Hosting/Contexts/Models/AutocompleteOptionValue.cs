using DiscoSdk.Contexts.Interactions;

namespace DiscoSdk.Hosting.Contexts.Models;

internal class AutocompleteOptionValue(string name, object? value) : IAutocompleteOptionValue
{
	public string Name => name;
	public object? Value => value;
}

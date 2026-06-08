using DiscoSdk.Contexts.Interactions;

namespace DiscoSdk.Hosting.Contexts.Models;

internal class AutoCompleteOptionValue(string name, object? value) : IAutoCompleteOptionValue
{
	public string Name => name;
	public object? Value => value;
}

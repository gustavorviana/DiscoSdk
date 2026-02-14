namespace DiscoSdk.Contexts.Interactions;

/// <summary>
/// Represents a single option name/value pair in an autocomplete context (non-focused options already filled by the user).
/// </summary>
public interface IAutocompleteOptionValue
{
	/// <summary>
	/// Gets the option name.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Gets the option value.
	/// </summary>
	object? Value { get; }
}

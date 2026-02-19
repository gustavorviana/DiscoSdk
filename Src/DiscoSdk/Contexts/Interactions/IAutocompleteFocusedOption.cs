using DiscoSdk.Models.Enums;

namespace DiscoSdk.Contexts.Interactions;

/// <summary>
/// Represents the option that is currently focused (being typed) in an autocomplete interaction.
/// </summary>
public interface IAutocompleteFocusedOption
{
	/// <summary>
	/// Gets the name of the option.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Gets the type of the option (String, Integer, Number, etc.).
	/// </summary>
	SlashCommandOptionType Type { get; }

	/// <summary>
	/// Gets the current partial value entered by the user (string for STRING, number for INTEGER/NUMBER, etc.).
	/// </summary>
	object? Value { get; }
}

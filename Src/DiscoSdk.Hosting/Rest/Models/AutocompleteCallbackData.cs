using DiscoSdk.Models.Commands;
using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Rest.Models;

/// <summary>
/// Represents the data payload for an application command autocomplete response (callback type 8).
/// </summary>
public class AutocompleteCallbackData
{
	/// <summary>
	/// Gets or sets the autocomplete choices (max 25).
	/// </summary>
	[JsonPropertyName("choices")]
	public SlashCommandOptionChoice[] Choices { get; set; } = [];
}

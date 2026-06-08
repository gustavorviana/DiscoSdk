using DiscoSdk.Models.Commands;
using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Rest.Models;

/// <summary>
/// Represents the data payload for an application command AutoComplete response (callback type 8).
/// </summary>
public class AutoCompleteCallbackData
{
	/// <summary>
	/// Gets or sets the AutoComplete choices (max 25).
	/// </summary>
	[JsonPropertyName("choices")]
	public SlashCommandOptionChoice[] Choices { get; set; } = [];
}

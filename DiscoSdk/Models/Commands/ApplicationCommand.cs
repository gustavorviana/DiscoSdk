using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Commands;

/// <summary>
/// Represents a Discord application command (slash command).
/// </summary>
public class ApplicationCommand
{
	/// <summary>
	/// Gets or sets the unique ID of the command.
	/// </summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>
	/// Gets or sets the type of command.
	/// </summary>
	[JsonPropertyName("type")]
	public int? Type { get; set; }

	/// <summary>
	/// Gets or sets the name of the command (1-32 characters, lowercase).
	/// </summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = default!;

	/// <summary>
	/// Gets or sets the localization dictionary for the name field.
	/// </summary>
	[JsonPropertyName("name_localizations")]
	public Dictionary<string, string>? NameLocalizations { get; set; }

	/// <summary>
	/// Gets or sets the description of the command (1-100 characters).
	/// </summary>
	[JsonPropertyName("description")]
	public string Description { get; set; } = default!;

	/// <summary>
	/// Gets or sets the localization dictionary for the description field.
	/// </summary>
	[JsonPropertyName("description_localizations")]
	public Dictionary<string, string>? DescriptionLocalizations { get; set; }

	/// <summary>
	/// Gets or sets the parameters for the command.
	/// </summary>
	[JsonPropertyName("options")]
	public List<ApplicationCommandOption>? Options { get; set; }

	/// <summary>
	/// Gets or sets the default member permissions required to use the command.
	/// </summary>
	[JsonPropertyName("default_member_permissions")]
	public string? DefaultMemberPermissions { get; set; }

	/// <summary>
	/// Gets or sets whether the command is available in DMs.
	/// </summary>
	[JsonPropertyName("dm_permission")]
	public bool? DmPermission { get; set; }

	/// <summary>
	/// Gets or sets whether the command is age-restricted.
	/// </summary>
	[JsonPropertyName("nsfw")]
	public bool? Nsfw { get; set; }

	/// <summary>
	/// Gets or sets the version of the command.
	/// </summary>
	[JsonPropertyName("version")]
	public string? Version { get; set; }
}

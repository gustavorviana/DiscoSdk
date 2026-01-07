using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Messages;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents resolved data from an interaction.
/// </summary>
public class InteractionResolved
{
	/// <summary>
	/// Gets or sets the resolved users.
	/// </summary>
	[JsonPropertyName("users")]
	public Dictionary<string, User>? Users { get; set; }

	/// <summary>
	/// Gets or sets the resolved members.
	/// </summary>
	[JsonPropertyName("members")]
	public Dictionary<string, GuildMember>? Members { get; set; }

	/// <summary>
	/// Gets or sets the resolved roles.
	/// </summary>
	[JsonPropertyName("roles")]
	public Dictionary<string, Role>? Roles { get; set; }

	/// <summary>
	/// Gets or sets the resolved channels.
	/// </summary>
	[JsonPropertyName("channels")]
	public Dictionary<string, Channel>? Channels { get; set; }

	/// <summary>
	/// Gets or sets the resolved messages.
	/// </summary>
	[JsonPropertyName("messages")]
	public Dictionary<string, Message>? Messages { get; set; }
}
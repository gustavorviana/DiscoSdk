using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Users;
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

	/// <summary>
	/// Resolved attachments — populated by Discord when a modal submission includes a
	/// <c>FileUpload</c> component. Keyed by attachment ID (the same IDs that appear in the
	/// submitted field's <c>Values</c> array).
	/// </summary>
	[JsonPropertyName("attachments")]
	public Dictionary<string, Attachment>? Attachments { get; set; }
}
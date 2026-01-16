using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a member of a Discord thread.
/// </summary>
public class ThreadMember
{
	/// <summary>
	/// Gets or sets the ID of the thread.
	/// </summary>
	[JsonPropertyName("id")]
	public Snowflake? Id { get; set; }

	/// <summary>
	/// Gets or sets the ID of the user.
	/// </summary>
	[JsonPropertyName("user_id")]
	public Snowflake? UserId { get; set; }

	/// <summary>
	/// Gets or sets when the user joined the thread.
	/// </summary>
	[JsonPropertyName("join_timestamp")]
	public string JoinTimestamp { get; set; } = default!;

	/// <summary>
	/// Gets or sets any user-thread settings, currently only used for notifications.
	/// </summary>
	[JsonPropertyName("flags")]
	public ThreadMemberFlags Flags { get; set; }

	/// <summary>
	/// Gets or sets additional information about the member.
	/// </summary>
	[JsonPropertyName("member")]
	public GuildMember? Member { get; set; }

	/// <summary>
	/// Gets or sets the presence information for the member.
	/// </summary>
	[JsonPropertyName("presence")]
	public object? Presence { get; set; }
}


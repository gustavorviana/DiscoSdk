using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Users;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a Discord invite.
/// </summary>
public class Invite
{
	/// <summary>
	/// Gets or sets the invite code.
	/// </summary>
	[JsonPropertyName("code")]
	public string Code { get; set; } = default!;

	/// <summary>
	/// Gets or sets the channel this invite is for.
	/// </summary>
	[JsonPropertyName("channel")]
	public Channel? Channel { get; set; }

	/// <summary>
	/// Gets or sets the guild this invite is for, or null if it's for a group DM.
	/// </summary>
	[JsonPropertyName("guild")]
	public Guild? Guild { get; set; }

	/// <summary>
	/// Gets or sets the user who created this invite, or null if unknown.
	/// </summary>
	[JsonPropertyName("inviter")]
	public User? Inviter { get; set; }

	/// <summary>
	/// Gets or sets the target type of this invite (1 = STREAM, 2 = EMBEDDED_APPLICATION).
	/// </summary>
	[JsonPropertyName("target_type")]
	public int? TargetType { get; set; }

	/// <summary>
	/// Gets or sets the target user for this invite, if applicable.
	/// </summary>
	[JsonPropertyName("target_user")]
	public User? TargetUser { get; set; }

	/// <summary>
	/// Gets or sets the ID of the target application for this invite, if applicable.
	/// </summary>
	[JsonPropertyName("target_application_id")]
	public Snowflake? TargetApplicationId { get; set; }

	/// <summary>
	/// Gets or sets the approximate number of times this invite has been used.
	/// </summary>
	[JsonPropertyName("approximate_presence_count")]
	public int? ApproximatePresenceCount { get; set; }

	/// <summary>
	/// Gets or sets the approximate number of members in the guild.
	/// </summary>
	[JsonPropertyName("approximate_member_count")]
	public int? ApproximateMemberCount { get; set; }

	/// <summary>
	/// Gets or sets the number of times this invite has been used.
	/// </summary>
	[JsonPropertyName("uses")]
	public int? Uses { get; set; }

	/// <summary>
	/// Gets or sets the maximum number of times this invite can be used.
	/// </summary>
	[JsonPropertyName("max_uses")]
	public int? MaxUses { get; set; }

	/// <summary>
	/// Gets or sets the duration (in seconds) after which the invite expires, or null if it never expires.
	/// </summary>
	[JsonPropertyName("max_age")]
	public int? MaxAge { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether this invite is temporary (users will be kicked when they log out).
	/// </summary>
	[JsonPropertyName("temporary")]
	public bool? Temporary { get; set; }

	/// <summary>
	/// Gets or sets the date and time when this invite was created.
	/// </summary>
	[JsonPropertyName("created_at")]
	public string? CreatedAt { get; set; }

	/// <summary>
	/// Gets or sets the date and time when this invite expires, or null if it never expires.
	/// </summary>
	[JsonPropertyName("expires_at")]
	public string? ExpiresAt { get; set; }
}
using DiscoSdk.Models.Channels;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a Discord invite.
/// </summary>
public interface IInvite : IDeletable
{
	/// <summary>
	/// Gets the invite code.
	/// </summary>
	string Code { get; }

	/// <summary>
	/// Gets the channel this invite is for.
	/// </summary>
	IChannel Channel { get; }

	/// <summary>
	/// Gets the guild this invite is for, or null if it's for a group DM.
	/// </summary>
	IGuild Guild { get; }

	/// <summary>
	/// Gets the user who created this invite, or null if unknown.
	/// </summary>
	IUser? Inviter { get; }

	/// <summary>
	/// Gets the target type of this invite (1 = STREAM, 2 = EMBEDDED_APPLICATION).
	/// </summary>
	int? TargetType { get; }

	/// <summary>
	/// Gets the target user for this invite, if applicable.
	/// </summary>
	IUser? TargetUser { get; }

	/// <summary>
	/// Gets the ID of the target application for this invite, if applicable.
	/// </summary>
	Snowflake? TargetApplicationId { get; }

	/// <summary>
	/// Gets the approximate number of times this invite has been used.
	/// </summary>
	int? ApproximatePresenceCount { get; }

	/// <summary>
	/// Gets the approximate number of members in the guild.
	/// </summary>
	int? ApproximateMemberCount { get; }

	/// <summary>
	/// Gets the number of times this invite has been used.
	/// </summary>
	int? Uses { get; }

	/// <summary>
	/// Gets the maximum number of times this invite can be used.
	/// </summary>
	int? MaxUses { get; }

	/// <summary>
	/// Gets the duration (in seconds) after which the invite expires, or null if it never expires.
	/// </summary>
	int? MaxAge { get; }

	/// <summary>
	/// Gets a value indicating whether this invite is temporary (users will be kicked when they log out).
	/// </summary>
	bool? Temporary { get; }

	/// <summary>
	/// Gets the date and time when this invite was created.
	/// </summary>
	DateTimeOffset? CreatedAt { get; }

	/// <summary>
	/// Gets the date and time when this invite expires, or null if it never expires.
	/// </summary>
	DateTimeOffset? ExpiresAt { get; }

	/// <summary>
	/// Gets a value indicating whether this invite has expired.
	/// </summary>
	bool IsExpired { get; }

	/// <summary>
	/// Gets the full URL for this invite.
	/// </summary>
	string Url { get; }

	/// <summary>
	/// Gets a value indicating whether this invite has reached its maximum uses.
	/// </summary>
	bool IsMaxedOut { get; }
}

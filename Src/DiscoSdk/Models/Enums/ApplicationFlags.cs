namespace DiscoSdk.Models.Enums;

/// <summary>
/// Public bit flags on a Discord application.
/// </summary>
[Flags]
public enum ApplicationFlags
{
	/// <summary>No flags set.</summary>
	None = 0,

	/// <summary>Indicates if the app uses the Auto Moderation API and has created at least 100 rules in the last 30 days.</summary>
	ApplicationAutoModerationRuleCreateBadge = 1 << 6,

	/// <summary>Intent required for bots in 100+ guilds to receive presence update events.</summary>
	GatewayPresence = 1 << 12,

	/// <summary>Intent required for bots in under 100 guilds to receive presence update events (found in the Developer Portal).</summary>
	GatewayPresenceLimited = 1 << 13,

	/// <summary>Intent required for bots in 100+ guilds to receive member-related events.</summary>
	GatewayGuildMembers = 1 << 14,

	/// <summary>Intent required for bots in under 100 guilds to receive member-related events (found in the Developer Portal).</summary>
	GatewayGuildMembersLimited = 1 << 15,

	/// <summary>Indicates unusual growth of an app that prevents verification.</summary>
	VerificationPendingGuildLimit = 1 << 16,

	/// <summary>Indicates if an app is embedded within the Discord client (currently unavailable publicly).</summary>
	Embedded = 1 << 17,

	/// <summary>Intent required for bots in 100+ guilds to receive message content.</summary>
	GatewayMessageContent = 1 << 18,

	/// <summary>Intent required for bots in under 100 guilds to receive message content (found in the Developer Portal).</summary>
	GatewayMessageContentLimited = 1 << 19,

	/// <summary>Indicates if an app has registered global application commands.</summary>
	ApplicationCommandBadge = 1 << 23
}

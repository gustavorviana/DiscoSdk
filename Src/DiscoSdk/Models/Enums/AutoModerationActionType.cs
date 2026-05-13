namespace DiscoSdk.Models.Enums;

/// <summary>
/// The type of action executed when an auto-moderation rule is triggered.
/// </summary>
public enum AutoModerationActionType
{
	/// <summary>Block the message and optionally show the member a custom explanation.</summary>
	BlockMessage = 1,

	/// <summary>Log the matched content to a channel.</summary>
	SendAlertMessage = 2,

	/// <summary>Time the member out for a duration (requires the <c>KEYWORD</c> or <c>MENTION_SPAM</c> trigger).</summary>
	Timeout = 3,

	/// <summary>Prevent the member from using text, voice or other interactions.</summary>
	BlockMemberInteraction = 4
}

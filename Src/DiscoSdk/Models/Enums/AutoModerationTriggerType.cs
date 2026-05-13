namespace DiscoSdk.Models.Enums;

/// <summary>
/// The type of content that triggers an auto-moderation rule.
/// </summary>
public enum AutoModerationTriggerType
{
	/// <summary>Check if content contains words from a user-defined list of keywords.</summary>
	Keyword = 1,

	/// <summary>Check if content represents generic spam.</summary>
	Spam = 3,

	/// <summary>Check if content contains words from internal pre-defined word sets.</summary>
	KeywordPreset = 4,

	/// <summary>Check if content contains more unique mentions than allowed.</summary>
	MentionSpam = 5,

	/// <summary>Check if a member's profile contains words from a user-defined list of keywords.</summary>
	MemberProfile = 6
}

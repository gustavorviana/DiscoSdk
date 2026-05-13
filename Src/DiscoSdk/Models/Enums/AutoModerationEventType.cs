namespace DiscoSdk.Models.Enums;

/// <summary>
/// The context in which an auto-moderation rule is checked.
/// </summary>
public enum AutoModerationEventType
{
	/// <summary>A member sends or edits a message in the guild.</summary>
	MessageSend = 1,

	/// <summary>A member edits their profile.</summary>
	MemberUpdate = 2
}

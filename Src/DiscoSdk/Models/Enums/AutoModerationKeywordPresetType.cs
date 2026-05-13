namespace DiscoSdk.Models.Enums;

/// <summary>
/// An internally pre-defined word set used by a <see cref="AutoModerationTriggerType.KeywordPreset"/> rule.
/// </summary>
public enum AutoModerationKeywordPresetType
{
	/// <summary>Words that may be considered profanity.</summary>
	Profanity = 1,

	/// <summary>Words that refer to sexually explicit behaviour or activity.</summary>
	SexualContent = 2,

	/// <summary>Personal insults or words that may be considered hate speech.</summary>
	Slurs = 3
}

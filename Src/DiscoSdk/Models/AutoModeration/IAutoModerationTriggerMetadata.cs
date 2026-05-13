using DiscoSdk.Models.Enums;

namespace DiscoSdk.Models.AutoModeration;

/// <summary>
/// Read-only view of the trigger metadata attached to an <see cref="IAutoModerationRule"/>.
/// </summary>
public interface IAutoModerationTriggerMetadata
{
	/// <summary>Substrings which will be searched for in content (KEYWORD / MEMBER_PROFILE).</summary>
	IReadOnlyList<string>? KeywordFilter { get; }

	/// <summary>Regular expression patterns which will be matched against content (KEYWORD / MEMBER_PROFILE).</summary>
	IReadOnlyList<string>? RegexPatterns { get; }

	/// <summary>The internally pre-defined word sets which will be searched for in content (KEYWORD_PRESET).</summary>
	IReadOnlyList<AutoModerationKeywordPresetType>? Presets { get; }

	/// <summary>Substrings which should not trigger the rule (KEYWORD / KEYWORD_PRESET / MEMBER_PROFILE).</summary>
	IReadOnlyList<string>? AllowList { get; }

	/// <summary>Total number of unique role and user mentions allowed per message (MENTION_SPAM).</summary>
	int? MentionTotalLimit { get; }

	/// <summary>Whether to automatically detect mention raids (MENTION_SPAM).</summary>
	bool? MentionRaidProtectionEnabled { get; }
}

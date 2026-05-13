using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.AutoModeration;

/// <summary>
/// Additional data used to determine whether an auto-moderation rule should be triggered. Which fields
/// are present depends on the rule's <see cref="AutoModerationRule.TriggerType"/>. Doubles as the
/// public read surface (<see cref="IAutoModerationTriggerMetadata"/>).
/// </summary>
public class AutoModerationTriggerMetadata : IAutoModerationTriggerMetadata
{
	/// <summary>Substrings which will be searched for in content (KEYWORD / MEMBER_PROFILE).</summary>
	[JsonPropertyName("keyword_filter")]
	public string[]? KeywordFilter { get; set; }

	/// <summary>Regular expression patterns which will be matched against content (KEYWORD / MEMBER_PROFILE).</summary>
	[JsonPropertyName("regex_patterns")]
	public string[]? RegexPatterns { get; set; }

	/// <summary>The internally pre-defined word sets which will be searched for in content (KEYWORD_PRESET).</summary>
	[JsonPropertyName("presets")]
	public AutoModerationKeywordPresetType[]? Presets { get; set; }

	/// <summary>Substrings which should not trigger the rule (KEYWORD / KEYWORD_PRESET / MEMBER_PROFILE).</summary>
	[JsonPropertyName("allow_list")]
	public string[]? AllowList { get; set; }

	/// <summary>Total number of unique role and user mentions allowed per message (MENTION_SPAM, max 50).</summary>
	[JsonPropertyName("mention_total_limit")]
	public int? MentionTotalLimit { get; set; }

	/// <summary>Whether to automatically detect mention raids (MENTION_SPAM).</summary>
	[JsonPropertyName("mention_raid_protection_enabled")]
	public bool? MentionRaidProtectionEnabled { get; set; }

	IReadOnlyList<string>? IAutoModerationTriggerMetadata.KeywordFilter => KeywordFilter;
	IReadOnlyList<string>? IAutoModerationTriggerMetadata.RegexPatterns => RegexPatterns;
	IReadOnlyList<AutoModerationKeywordPresetType>? IAutoModerationTriggerMetadata.Presets => Presets;
	IReadOnlyList<string>? IAutoModerationTriggerMetadata.AllowList => AllowList;
}

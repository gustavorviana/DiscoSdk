using DiscoSdk.Models;
using DiscoSdk.Models.AutoModeration;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// A REST action that creates an auto-moderation rule. The name, event type and trigger type are
/// provided up front; everything else is optional.
/// </summary>
public interface ICreateAutoModerationRuleAction : IRestAction<IAutoModerationRule>
{
	/// <summary>Sets the trigger metadata for the rule (required for KEYWORD, KEYWORD_PRESET, MENTION_SPAM and MEMBER_PROFILE triggers).</summary>
	ICreateAutoModerationRuleAction SetTriggerMetadata(AutoModerationTriggerMetadata metadata);

	/// <summary>Sets the actions executed when the rule is triggered (at least one required).</summary>
	ICreateAutoModerationRuleAction SetActions(params AutoModerationAction[] actions);

	/// <summary>Sets whether the rule is enabled (defaults to <c>false</c> on Discord's side if omitted).</summary>
	ICreateAutoModerationRuleAction SetEnabled(bool enabled = true);

	/// <summary>Sets the roles that are exempt from the rule (max 20).</summary>
	ICreateAutoModerationRuleAction SetExemptRoles(params Snowflake[] roleIds);

	/// <summary>Sets the channels that are exempt from the rule (max 50).</summary>
	ICreateAutoModerationRuleAction SetExemptChannels(params Snowflake[] channelIds);
}

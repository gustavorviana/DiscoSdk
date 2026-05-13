using DiscoSdk.Models;
using DiscoSdk.Models.AutoModeration;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// A REST action that modifies an existing auto-moderation rule. Every field is optional — only the
/// ones that are set are sent.
/// </summary>
public interface IModifyAutoModerationRuleAction : IRestAction<IAutoModerationRule>
{
	/// <summary>Sets a new name for the rule.</summary>
	IModifyAutoModerationRuleAction SetName(string name);

	/// <summary>Sets the event context in which the rule is checked.</summary>
	IModifyAutoModerationRuleAction SetEventType(AutoModerationEventType eventType);

	/// <summary>Sets the trigger metadata for the rule.</summary>
	IModifyAutoModerationRuleAction SetTriggerMetadata(AutoModerationTriggerMetadata metadata);

	/// <summary>Sets the actions executed when the rule is triggered.</summary>
	IModifyAutoModerationRuleAction SetActions(params AutoModerationAction[] actions);

	/// <summary>Sets whether the rule is enabled.</summary>
	IModifyAutoModerationRuleAction SetEnabled(bool enabled);

	/// <summary>Sets the roles that are exempt from the rule.</summary>
	IModifyAutoModerationRuleAction SetExemptRoles(params Snowflake[] roleIds);

	/// <summary>Sets the channels that are exempt from the rule.</summary>
	IModifyAutoModerationRuleAction SetExemptChannels(params Snowflake[] channelIds);
}

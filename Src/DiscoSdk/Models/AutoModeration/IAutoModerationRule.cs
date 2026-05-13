using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.AutoModeration;

/// <summary>
/// A Discord auto-moderation rule, with the operations that can be performed on it.
/// </summary>
public interface IAutoModerationRule
{
	/// <summary>The ID of this rule.</summary>
	Snowflake Id { get; }

	/// <summary>The ID of the guild this rule belongs to.</summary>
	Snowflake GuildId { get; }

	/// <summary>The rule name.</summary>
	string Name { get; }

	/// <summary>The ID of the user who created this rule.</summary>
	Snowflake CreatorId { get; }

	/// <summary>The event context in which the rule is checked.</summary>
	AutoModerationEventType EventType { get; }

	/// <summary>The type of content which triggers the rule.</summary>
	AutoModerationTriggerType TriggerType { get; }

	/// <summary>Additional data used to determine whether the rule is triggered.</summary>
	IAutoModerationTriggerMetadata? TriggerMetadata { get; }

	/// <summary>The actions executed when the rule is triggered.</summary>
	IReadOnlyList<IAutoModerationAction> Actions { get; }

	/// <summary>Whether the rule is enabled.</summary>
	bool Enabled { get; }

	/// <summary>The role IDs that are not affected by the rule.</summary>
	IReadOnlyList<Snowflake> ExemptRoles { get; }

	/// <summary>The channel IDs that are not affected by the rule.</summary>
	IReadOnlyList<Snowflake> ExemptChannels { get; }

	/// <summary>Creates a REST action that modifies this rule.</summary>
	IModifyAutoModerationRuleAction Modify();

	/// <summary>Creates a REST action that deletes this rule.</summary>
	IRestAction Delete();
}

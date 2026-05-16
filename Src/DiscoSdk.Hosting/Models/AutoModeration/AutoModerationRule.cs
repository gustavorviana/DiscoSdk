using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.AutoModeration;

/// <summary>
/// Represents a Discord auto-moderation rule — a guild-configured check that scans content and runs
/// actions when triggered.
/// </summary>
internal class AutoModerationRule
{
	/// <summary>The ID of this rule.</summary>
	[JsonPropertyName("id")]
	public Snowflake Id { get; set; } = default!;

	/// <summary>The ID of the guild this rule belongs to.</summary>
	[JsonPropertyName("guild_id")]
	public Snowflake GuildId { get; set; } = default!;

	/// <summary>The rule name.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = default!;

	/// <summary>The ID of the user who created this rule.</summary>
	[JsonPropertyName("creator_id")]
	public Snowflake CreatorId { get; set; } = default!;

	/// <summary>The event context in which the rule is checked.</summary>
	[JsonPropertyName("event_type")]
	public AutoModerationEventType EventType { get; set; }

	/// <summary>The type of content which triggers the rule.</summary>
	[JsonPropertyName("trigger_type")]
	public AutoModerationTriggerType TriggerType { get; set; }

	/// <summary>Additional data used to determine whether the rule is triggered.</summary>
	[JsonPropertyName("trigger_metadata")]
	public AutoModerationTriggerMetadata? TriggerMetadata { get; set; }

	/// <summary>The actions executed when the rule is triggered.</summary>
	[JsonPropertyName("actions")]
	public AutoModerationAction[] Actions { get; set; } = [];

	/// <summary>Whether the rule is enabled.</summary>
	[JsonPropertyName("enabled")]
	public bool Enabled { get; set; }

	/// <summary>The role IDs that are not affected by the rule (max 20).</summary>
	[JsonPropertyName("exempt_roles")]
	public Snowflake[] ExemptRoles { get; set; } = [];

	/// <summary>The channel IDs that are not affected by the rule (max 50).</summary>
	[JsonPropertyName("exempt_channels")]
	public Snowflake[] ExemptChannels { get; set; } = [];
}

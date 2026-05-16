using DiscoSdk.Models;
using DiscoSdk.Models.AutoModeration;

namespace DiscoSdk.Contexts.Guilds;

/// <summary>
/// Context for the <c>AUTO_MODERATION_ACTION_EXECUTION</c> Gateway event — an auto-moderation rule
/// fired and Discord took (or attempted) an action against a user.
/// </summary>
public interface IAutoModerationActionExecutionContext : IContext
{
	/// <summary>The guild in which the action was executed.</summary>
	IGuild Guild { get; }

	/// <summary>The rule that triggered the action.</summary>
	Snowflake RuleId { get; }

	/// <summary>The action that was executed.</summary>
	IAutoModerationAction Action { get; }

	/// <summary>The user who triggered the rule.</summary>
	Snowflake UserId { get; }

	/// <summary>The channel where the offending message was sent, if applicable.</summary>
	Snowflake? ChannelId { get; }

	/// <summary>The matched content that triggered the rule (requires MESSAGE_CONTENT intent).</summary>
	string? MatchedContent { get; }
}

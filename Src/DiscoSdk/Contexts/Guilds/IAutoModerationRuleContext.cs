using DiscoSdk.Models;
using DiscoSdk.Models.AutoModeration;

namespace DiscoSdk.Contexts.Guilds;

/// <summary>
/// Context for <c>AUTO_MODERATION_RULE_CREATE</c>, <c>AUTO_MODERATION_RULE_UPDATE</c>, and
/// <c>AUTO_MODERATION_RULE_DELETE</c> Gateway events.
/// </summary>
public interface IAutoModerationRuleContext : IContext
{
	/// <summary>The auto-moderation rule.</summary>
	IAutoModerationRule Rule { get; }

	/// <summary>The guild owning the rule.</summary>
	IGuild Guild { get; }
}

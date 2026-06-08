using DiscoSdk.Models.AutoModeration;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models;

/// <summary>
/// Per-guild auto-moderation surface — every operation that targets
/// <c>/guilds/:id/auto-moderation/rules*</c>.
/// </summary>
public interface IGuildAutoModeration
{
    /// <summary>Builds a deferred REST action that lists this guild's auto-moderation rules.</summary>
    IRestAction<IReadOnlyList<IAutoModerationRule>> GetAll();

    /// <summary>Builds a deferred REST action that retrieves a single auto-moderation rule by id.</summary>
    IRestAction<IAutoModerationRule> Get(Snowflake ruleId);

    /// <summary>
    /// Builds a deferred fluent action that creates a new auto-moderation rule. Configure trigger
    /// metadata, actions, etc. on the returned action before executing it.
    /// </summary>
    ICreateAutoModerationRuleAction Create(string name, AutoModerationEventType eventType, AutoModerationTriggerType triggerType);
}

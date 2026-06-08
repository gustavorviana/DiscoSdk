using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.AutoModeration;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Surfaces;

internal sealed class GuildAutoModerationSurface(DiscordClient client, Snowflake guildId) : IGuildAutoModeration
{
    private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));

    public IRestAction<IReadOnlyList<IAutoModerationRule>> GetAll()
        => RestAction<IReadOnlyList<IAutoModerationRule>>.Create(async ct =>
        {
            var rules = await _client.AutoModerationClient.ListRulesAsync(guildId, ct).ConfigureAwait(false);
            return rules.Select(r => (IAutoModerationRule)new AutoModerationRuleWrapper(_client, r)).ToList().AsReadOnly();
        });

    public IRestAction<IAutoModerationRule> Get(Snowflake ruleId)
        => RestAction<IAutoModerationRule>.Create(async ct =>
            new AutoModerationRuleWrapper(_client, await _client.AutoModerationClient.GetRuleAsync(guildId, ruleId, ct).ConfigureAwait(false)));

    public ICreateAutoModerationRuleAction Create(string name, AutoModerationEventType eventType, AutoModerationTriggerType triggerType)
        => new CreateAutoModerationRuleAction(_client, guildId, name, eventType, triggerType);
}

using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.AutoModeration;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="IModifyAutoModerationRuleAction"/>.
/// </summary>
internal sealed class ModifyAutoModerationRuleAction(DiscordClient client, Snowflake guildId, Snowflake ruleId)
	: RestAction<IAutoModerationRule>, IModifyAutoModerationRuleAction
{
	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
	private readonly Dictionary<string, object?> _changes = [];

	public IModifyAutoModerationRuleAction SetName(string name)
	{
		_changes["name"] = name ?? throw new ArgumentNullException(nameof(name));
		return this;
	}

	public IModifyAutoModerationRuleAction SetEventType(AutoModerationEventType eventType)
	{
		_changes["event_type"] = (int)eventType;
		return this;
	}

	public IModifyAutoModerationRuleAction SetTriggerMetadata(AutoModerationTriggerMetadata metadata)
	{
		_changes["trigger_metadata"] = metadata ?? throw new ArgumentNullException(nameof(metadata));
		return this;
	}

	public IModifyAutoModerationRuleAction SetActions(params AutoModerationAction[] actions)
	{
		_changes["actions"] = actions;
		return this;
	}

	public IModifyAutoModerationRuleAction SetEnabled(bool enabled)
	{
		_changes["enabled"] = enabled;
		return this;
	}

	public IModifyAutoModerationRuleAction SetExemptRoles(params Snowflake[] roleIds)
	{
		_changes["exempt_roles"] = roleIds?.Select(r => r.ToString()).ToArray();
		return this;
	}

	public IModifyAutoModerationRuleAction SetExemptChannels(params Snowflake[] channelIds)
	{
		_changes["exempt_channels"] = channelIds?.Select(c => c.ToString()).ToArray();
		return this;
	}

	public override async Task<IAutoModerationRule> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		var rule = await _client.AutoModerationClient.ModifyRuleAsync(guildId, ruleId, _changes, cancellationToken);
		return new AutoModerationRuleWrapper(_client, rule);
	}
}

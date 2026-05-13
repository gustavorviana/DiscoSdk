using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.AutoModeration;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="ICreateAutoModerationRuleAction"/>.
/// </summary>
internal sealed class CreateAutoModerationRuleAction : RestAction<IAutoModerationRule>, ICreateAutoModerationRuleAction
{
	private readonly DiscordClient _client;
	private readonly Snowflake _guildId;
	private readonly string _name;
	private readonly AutoModerationEventType _eventType;
	private readonly AutoModerationTriggerType _triggerType;
	private AutoModerationTriggerMetadata? _triggerMetadata;
	private AutoModerationAction[]? _actions;
	private bool? _enabled;
	private Snowflake[]? _exemptRoles;
	private Snowflake[]? _exemptChannels;

	public CreateAutoModerationRuleAction(DiscordClient client, Snowflake guildId, string name, AutoModerationEventType eventType, AutoModerationTriggerType triggerType)
	{
		_client = client ?? throw new ArgumentNullException(nameof(client));
		_guildId = guildId;
		_name = name ?? throw new ArgumentNullException(nameof(name));
		_eventType = eventType;
		_triggerType = triggerType;
	}

	public ICreateAutoModerationRuleAction SetTriggerMetadata(AutoModerationTriggerMetadata metadata)
	{
		_triggerMetadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
		return this;
	}

	public ICreateAutoModerationRuleAction SetActions(params AutoModerationAction[] actions)
	{
		_actions = actions;
		return this;
	}

	public ICreateAutoModerationRuleAction SetEnabled(bool enabled = true)
	{
		_enabled = enabled;
		return this;
	}

	public ICreateAutoModerationRuleAction SetExemptRoles(params Snowflake[] roleIds)
	{
		_exemptRoles = roleIds;
		return this;
	}

	public ICreateAutoModerationRuleAction SetExemptChannels(params Snowflake[] channelIds)
	{
		_exemptChannels = channelIds;
		return this;
	}

	public override async Task<IAutoModerationRule> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		var request = new Dictionary<string, object?>
		{
			["name"] = _name,
			["event_type"] = (int)_eventType,
			["trigger_type"] = (int)_triggerType
		};

		if (_triggerMetadata != null)
			request["trigger_metadata"] = _triggerMetadata;
		if (_actions != null)
			request["actions"] = _actions;
		if (_enabled.HasValue)
			request["enabled"] = _enabled.Value;
		if (_exemptRoles != null)
			request["exempt_roles"] = _exemptRoles.Select(r => r.ToString()).ToArray();
		if (_exemptChannels != null)
			request["exempt_channels"] = _exemptChannels.Select(c => c.ToString()).ToArray();

		var rule = await _client.AutoModerationClient.CreateRuleAsync(_guildId, request, cancellationToken);
		return new AutoModerationRuleWrapper(_client, rule);
	}
}

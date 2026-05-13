using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Models.AutoModeration;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Wraps an <see cref="AutoModerationRule"/> model and exposes the operations available on the rule.
/// </summary>
internal sealed class AutoModerationRuleWrapper(DiscordClient client, AutoModerationRule model) : IAutoModerationRule
{
	private readonly AutoModerationRule _model = model ?? throw new ArgumentNullException(nameof(model));
	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));

	public Snowflake Id => _model.Id;
	public Snowflake GuildId => _model.GuildId;
	public string Name => _model.Name;
	public Snowflake CreatorId => _model.CreatorId;
	public AutoModerationEventType EventType => _model.EventType;
	public AutoModerationTriggerType TriggerType => _model.TriggerType;
	public IAutoModerationTriggerMetadata? TriggerMetadata => _model.TriggerMetadata;
	public IReadOnlyList<IAutoModerationAction> Actions => _model.Actions;
	public bool Enabled => _model.Enabled;
	public IReadOnlyList<Snowflake> ExemptRoles => _model.ExemptRoles;
	public IReadOnlyList<Snowflake> ExemptChannels => _model.ExemptChannels;

	public IModifyAutoModerationRuleAction Modify()
		=> new ModifyAutoModerationRuleAction(_client, _model.GuildId, _model.Id);

	public IRestAction Delete()
		=> RestAction.Create(ct => _client.AutoModerationClient.DeleteRuleAsync(_model.GuildId, _model.Id, ct));
}

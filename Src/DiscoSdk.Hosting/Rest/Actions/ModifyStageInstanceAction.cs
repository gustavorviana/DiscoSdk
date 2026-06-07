using DiscoSdk.Hosting.Models.Requests.StageInstances;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

internal sealed class ModifyStageInstanceAction(DiscordClient client, Snowflake channelId)
    : RestAction<IStageInstance>, IModifyStageInstanceAction
{
    private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly Snowflake _channelId = channelId;

    private string? _topic;
    private StagePrivacyLevel? _privacyLevel;
    private string? _reason;

    public IModifyStageInstanceAction SetTopic(string topic)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);
        if (topic.Length is < 1 or > 120)
            throw new ArgumentOutOfRangeException(nameof(topic), "Stage topic must be 1-120 characters.");

        _topic = topic;
        return this;
    }

    public IModifyStageInstanceAction SetPrivacyLevel(StagePrivacyLevel privacyLevel)
    {
        _privacyLevel = privacyLevel;
        return this;
    }

    public IModifyStageInstanceAction WithReason(string reason)
    {
        _reason = AuditLogReason.Validate(reason);
        return this;
    }

    public override async Task<IStageInstance> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (_topic is null && _privacyLevel is null)
            throw new InvalidOperationException("ModifyStageInstance requires at least one field (SetTopic or SetPrivacyLevel) before ExecuteAsync.");

        var request = new ModifyStageInstanceRequest
        {
            Topic = _topic,
            PrivacyLevel = _privacyLevel,
        };

        var updated = await _client.StageInstanceClient.ModifyAsync(_channelId, request, _reason, cancellationToken);
        return new StageInstanceWrapper(_client, updated);
    }
}

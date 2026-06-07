using DiscoSdk.Models;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Fluent builder for <c>PATCH /stage-instances/{channel.id}</c>. Only fields you touch are
/// sent on the wire. Discord records the change in the audit log when a reason is attached
/// via <see cref="IRestActionWithReason{TSelf}.WithReason"/>.
/// </summary>
public interface IModifyStageInstanceAction
    : IRestAction<IStageInstance>, IRestActionWithReason<IModifyStageInstanceAction>
{
    /// <summary>Updates the topic shown to listeners (1-120 chars).</summary>
    IModifyStageInstanceAction SetTopic(string topic);

    /// <summary>Updates who can join the stage.</summary>
    IModifyStageInstanceAction SetPrivacyLevel(StagePrivacyLevel privacyLevel);
}

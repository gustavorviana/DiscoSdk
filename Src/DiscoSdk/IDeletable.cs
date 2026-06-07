using DiscoSdk.Rest.Actions;

namespace DiscoSdk;

/// <summary>
/// Represents an object that can be deleted asynchronously.
/// </summary>
/// <remarks>
/// Entities whose deletion is recorded in the audit log (roles, webhooks, integrations, etc.) hide
/// this method with an override that returns <see cref="IReasonedRestAction"/> so the caller can
/// chain <see cref="IRestActionWithReason{TSelf}.WithReason"/>. Entities whose deletion is not
/// audit-loggable (reactions, group-DMs, application emojis) keep the plain <see cref="IRestAction"/>.
/// </remarks>
public interface IDeletable
{
    /// <summary>
    /// Gets a REST action for deleting this object.
    /// </summary>
    IRestAction Delete();
}

using DiscoSdk.Rest.Actions;

namespace DiscoSdk;

/// <summary>
/// Marker for entities whose deletion is recorded in the Discord audit log and accepts an
/// <c>X-Audit-Log-Reason</c> header. Extends <see cref="IDeletable"/> and shadows
/// <see cref="IDeletable.Delete"/> with a covariant return so callers that hold the stronger type
/// can chain <see cref="IRestActionWithReason{TSelf}.WithReason"/>; callers that only hold
/// <see cref="IDeletable"/> still observe a plain <see cref="IRestAction"/>.
/// </summary>
/// <remarks>
/// Implement this (instead of plain <see cref="IDeletable"/>) on any entity whose corresponding
/// Discord DELETE endpoint documents audit-log support: roles, guild channels, guild emoji,
/// stickers, webhooks, integrations, auto-moderation rules, scheduled events, invites, and
/// non-self message deletes. Leave the bare <see cref="IDeletable"/> on entities whose deletion
/// Discord does not record (reactions, DM channel closes, application emojis).
/// </remarks>
public interface IReasonedDeletable : IDeletable
{
    /// <summary>
    /// Gets a REST action for deleting this object. Chain
    /// <see cref="IRestActionWithReason{TSelf}.WithReason"/> to attach an audit-log reason.
    /// </summary>
    new IReasonedRestAction Delete();
}

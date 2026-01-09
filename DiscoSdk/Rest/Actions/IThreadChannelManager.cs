using DiscoSdk.Models;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a manager for thread channel operations.
/// </summary>
public interface IThreadChannelManager : IMessageChannelManager<IThreadChannelManager>
{
	/// <summary>
	/// Sets whether the thread is archived.
	/// </summary>
	/// <param name="archived">True to archive the thread, false to unarchive it.</param>
	/// <returns>The current <see cref="IThreadChannelManager"/> instance.</returns>
	IThreadChannelManager SetArchived(bool archived);

	/// <summary>
	/// Sets whether the thread is locked.
	/// </summary>
	/// <param name="locked">True to lock the thread, false to unlock it.</param>
	/// <returns>The current <see cref="IThreadChannelManager"/> instance.</returns>
	IThreadChannelManager SetLocked(bool locked);

	/// <summary>
	/// Sets whether non-moderators can add other non-moderators to the thread.
	/// </summary>
	/// <param name="invitable">True to allow inviting, false otherwise.</param>
	/// <returns>The current <see cref="IThreadChannelManager"/> instance.</returns>
	IThreadChannelManager SetInvitable(bool invitable);

	/// <summary>
	/// Sets the auto-archive duration for the thread.
	/// </summary>
	/// <param name="duration">The auto-archive duration.</param>
	/// <returns>The current <see cref="IThreadChannelManager"/> instance.</returns>
	IThreadChannelManager SetAutoArchiveDuration(ThreadAutoArchiveDuration duration);

	/// <summary>
	/// Sets the slowmode (rate limit per user) for the thread.
	/// </summary>
	/// <param name="slowmode">The slowmode duration, or null to disable slowmode.</param>
	/// <returns>The current <see cref="IThreadChannelManager"/> instance.</returns>
	IThreadChannelManager SetSlowmode(Slowmode? slowmode);
}


using DiscoSdk.Models;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a Discord channel that behaves like a thread (threads and media channels).
/// </summary>
public interface IThreadLikeChannel : IChannel
{
	/// <summary>
	/// Gets the number of messages in the thread.
	/// </summary>
	int? MessageCount { get; }

	/// <summary>
	/// Gets the approximate member count of the thread.
	/// </summary>
	int? MemberCount { get; }

	/// <summary>
	/// Gets thread-specific fields not needed by other channels.
	/// </summary>
	ThreadMetadata? ThreadMetadata { get; }

	/// <summary>
	/// Gets thread member object for the current user, if they have joined the thread.
	/// </summary>
	ThreadMember? Member { get; }

	/// <summary>
	/// Gets the total number of messages sent in the thread.
	/// </summary>
	int? TotalMessageSent { get; }

	/// <summary>
	/// Adds the current user to the thread.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task JoinThreadAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Removes the current user from the thread.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task LeaveThreadAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Adds a member to the thread.
	/// </summary>
	/// <param name="userId">The ID of the user to add.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task AddThreadMemberAsync(DiscordId userId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Removes a member from the thread.
	/// </summary>
	/// <param name="userId">The ID of the user to remove.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task RemoveThreadMemberAsync(DiscordId userId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets a thread member by their user ID.
	/// </summary>
	/// <param name="userId">The ID of the user to get the thread member for.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The thread member, or null if not found.</returns>
	Task<ThreadMember?> GetThreadMemberAsync(DiscordId userId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets a list of thread members in this thread.
	/// </summary>
	/// <param name="limit">Maximum number of thread members to return (1-100, default 25).</param>
	/// <param name="after">Get thread members after this user ID.</param>
	/// <param name="withMember">Whether to include guild member data.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>An array of thread members.</returns>
	Task<ThreadMember[]> GetThreadMembersAsync(int? limit = null, DiscordId? after = null, bool? withMember = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Archives this thread.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task ArchiveAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Unarchives this thread.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task UnarchiveAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Locks this thread.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task LockAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Unlocks this thread.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task UnlockAsync(CancellationToken cancellationToken = default);
}


using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a Discord guild text channel.
/// </summary>
public interface IGuildTextChannel : IGuildChannel, ITextBasedChannel
{
	/// <summary>
	/// Creates a builder for editing this channel.
	/// </summary>
	/// <returns>An <see cref="IEditGuildTextChannelRestAction"/> instance for editing the channel.</returns>
	IEditGuildTextChannelRestAction Edit();

	/// <summary>
	/// Gets the active threads in this channel.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>An array of active thread channels.</returns>
	Task<IChannel[]> GetActiveThreadsAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the public archived threads in this channel.
	/// </summary>
	/// <param name="before">Get threads before this timestamp.</param>
	/// <param name="limit">Maximum number of threads to return (1-100, default 50).</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>An array of archived thread channels.</returns>
	Task<IChannel[]> GetPublicArchivedThreadsAsync(string? before = null, int? limit = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the private archived threads in this channel (requires MANAGE_THREADS permission).
	/// </summary>
	/// <param name="before">Get threads before this timestamp.</param>
	/// <param name="limit">Maximum number of threads to return (1-100, default 50).</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>An array of archived thread channels.</returns>
	Task<IChannel[]> GetPrivateArchivedThreadsAsync(string? before = null, int? limit = null, CancellationToken cancellationToken = default);
}


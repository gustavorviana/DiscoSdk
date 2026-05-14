using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Gateway;

/// <summary>
/// Strategy that decides what to do with the <c>GUILD_MEMBERS_CHUNK</c> events that follow a
/// <c>Request Guild Members</c> (op 8) call. The coordinator routes each matching chunk to a
/// registered sink by nonce; the sink owns whether to buffer everything, stream per-member, or
/// anything else (e.g. write to disk).
/// </summary>
internal interface IMemberChunkSink
{
	/// <summary>
	/// Called for every chunk that matches this sink's nonce. The sink should detect the final
	/// chunk (<paramref name="chunkIndex"/> &gt;= <paramref name="chunkCount"/> - 1) and signal
	/// completion to its waiter.
	/// </summary>
	void OnChunk(IReadOnlyList<IMember> members, int chunkIndex, int chunkCount);

	/// <summary>
	/// Called by the coordinator when the action is cancelled (consumer break, token fired,
	/// exception in the send path). The sink should complete its waiter with an
	/// <see cref="OperationCanceledException"/>.
	/// </summary>
	void OnCancel(CancellationToken cancellationToken);
}

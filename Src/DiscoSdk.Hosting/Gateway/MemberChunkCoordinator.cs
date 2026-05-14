using DiscoSdk.Models;
using System.Collections.Concurrent;

namespace DiscoSdk.Hosting.Gateway;

/// <summary>
/// Nonce-keyed router that pairs incoming <c>GUILD_MEMBERS_CHUNK</c> events with the
/// <see cref="IMemberChunkSink"/> registered for the matching <c>Request Guild Members</c> (op 8)
/// call. The coordinator is policy-free — the sink owns what to do with the members
/// (buffer, stream, etc.).
/// </summary>
internal sealed class MemberChunkCoordinator
{
	private readonly ConcurrentDictionary<string, IMemberChunkSink> _sinks = new();

	/// <summary>
	/// Reserves <paramref name="nonce"/> for the supplied <paramref name="sink"/>. The caller
	/// must eventually drive the sink to completion (last chunk arrives) or call
	/// <see cref="Cancel"/> with the same nonce to free the slot.
	/// </summary>
	public void Register(string nonce, IMemberChunkSink sink)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(nonce);
		ArgumentNullException.ThrowIfNull(sink);

		if (!_sinks.TryAdd(nonce, sink))
			throw new InvalidOperationException($"A request with nonce '{nonce}' is already in flight.");
	}

	/// <summary>
	/// Routes a chunk to the sink registered under <paramref name="nonce"/>. No-op when nonce is
	/// missing/empty (the chunk wasn't ours) or when no sink is registered (request already
	/// completed/cancelled). On the final chunk the entry is removed automatically.
	/// </summary>
	public void TryDeliver(string? nonce, IReadOnlyList<IMember> members, int chunkIndex, int chunkCount)
	{
		if (string.IsNullOrEmpty(nonce)) return;
		if (!_sinks.TryGetValue(nonce, out var sink)) return;

		sink.OnChunk(members, chunkIndex, chunkCount);

		if (chunkIndex >= chunkCount - 1)
			_sinks.TryRemove(nonce, out _);
	}

	/// <summary>
	/// Removes the sink for <paramref name="nonce"/> and notifies it of cancellation. Safe to call
	/// when there's nothing registered for the nonce (e.g. the request already completed).
	/// </summary>
	public void Cancel(string nonce, CancellationToken cancellationToken = default)
	{
		if (_sinks.TryRemove(nonce, out var sink))
			sink.OnCancel(cancellationToken);
	}
}

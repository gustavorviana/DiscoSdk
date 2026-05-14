using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Gateway;

/// <summary>
/// Sink that accumulates every received chunk into a single list and completes its
/// <see cref="Completion"/> task on the final chunk. Used by <c>GetAsync</c>.
/// </summary>
internal sealed class BufferingMemberChunkSink : IMemberChunkSink
{
	private readonly List<IMember> _accumulated = [];
	private readonly TaskCompletionSource<IReadOnlyList<IMember>> _tcs =
		new(TaskCreationOptions.RunContinuationsAsynchronously);

	/// <summary>Completes with the full member list once the last chunk arrives.</summary>
	public Task<IReadOnlyList<IMember>> Completion => _tcs.Task;

	/// <inheritdoc />
	public void OnChunk(IReadOnlyList<IMember> members, int chunkIndex, int chunkCount)
	{
		_accumulated.AddRange(members);

		if (chunkIndex >= chunkCount - 1)
			_tcs.TrySetResult(_accumulated.AsReadOnly());
	}

	/// <inheritdoc />
	public void OnCancel(CancellationToken cancellationToken)
		=> _tcs.TrySetCanceled(cancellationToken);
}

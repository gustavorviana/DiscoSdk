using DiscoSdk.Models;
using System.Threading.Channels;

namespace DiscoSdk.Hosting.Gateway;

/// <summary>
/// Sink that pipes every received member into an unbounded channel, one item at a time. Used by
/// <c>StreamAsync</c> so callers can <c>await foreach</c> members as they arrive and break early
/// without buffering more than what Discord has already sent.
/// </summary>
internal sealed class StreamingMemberChunkSink : IMemberChunkSink
{
	private readonly Channel<IMember> _channel = Channel.CreateUnbounded<IMember>(
		new UnboundedChannelOptions { SingleReader = true, SingleWriter = true });

	/// <summary>The reader the action drains with <c>ReadAllAsync</c>.</summary>
	public ChannelReader<IMember> Reader => _channel.Reader;

	/// <inheritdoc />
	public void OnChunk(IReadOnlyList<IMember> members, int chunkIndex, int chunkCount)
	{
		foreach (var member in members)
			_channel.Writer.TryWrite(member);

		if (chunkIndex >= chunkCount - 1)
			_channel.Writer.TryComplete();
	}

	/// <inheritdoc />
	public void OnCancel(CancellationToken cancellationToken)
		=> _channel.Writer.TryComplete(new OperationCanceledException(cancellationToken));
}

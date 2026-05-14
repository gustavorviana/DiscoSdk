using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Models;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Gateway;

public class StreamingMemberChunkSinkTests
{
	[Fact]
	public async Task OnChunk_WritesEachMemberIndividuallyAsync()
	{
		var sut = new StreamingMemberChunkSink();

		var batch = new[] { Substitute.For<IMember>(), Substitute.For<IMember>(), Substitute.For<IMember>() };
		sut.OnChunk(batch, chunkIndex: 0, chunkCount: 1);  // single-chunk session completes the channel

		var received = new List<IMember>();
		await foreach (var member in sut.Reader.ReadAllAsync().WithCancellation(default))
			received.Add(member);

		Assert.Equal(3, received.Count);
		Assert.Equal(batch, received);
	}

	[Fact]
	public async Task OnChunk_FinalChunk_CompletesChannelAsync()
	{
		var sut = new StreamingMemberChunkSink();

		sut.OnChunk([Substitute.For<IMember>()], chunkIndex: 0, chunkCount: 2);
		sut.OnChunk([Substitute.For<IMember>()], chunkIndex: 1, chunkCount: 2);

		var count = 0;
		await foreach (var _ in sut.Reader.ReadAllAsync())
			count++;

		Assert.Equal(2, count);
	}

	[Fact]
	public async Task OnCancel_CompletesChannelWithCancellationAsync()
	{
		var sut = new StreamingMemberChunkSink();
		sut.OnChunk([Substitute.For<IMember>()], chunkIndex: 0, chunkCount: 5);

		using var cts = new CancellationTokenSource();
		cts.Cancel();
		sut.OnCancel(cts.Token);

		await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
		{
			await foreach (var _ in sut.Reader.ReadAllAsync())
			{
				// drain — should throw before exhausting
			}
		});
	}
}

using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Models;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Gateway;

public class BufferingMemberChunkSinkTests
{
	[Fact]
	public async Task OnChunk_AccumulatesAcrossChunks_AndCompletesOnFinalAsync()
	{
		var sut = new BufferingMemberChunkSink();

		sut.OnChunk([Substitute.For<IMember>(), Substitute.For<IMember>()], chunkIndex: 0, chunkCount: 2);
		Assert.False(sut.Completion.IsCompleted);

		sut.OnChunk([Substitute.For<IMember>()], chunkIndex: 1, chunkCount: 2);

		var members = await sut.Completion.WaitAsync(TimeSpan.FromSeconds(1));
		Assert.Equal(3, members.Count);
	}

	[Fact]
	public async Task OnChunk_SingleChunkSession_CompletesImmediatelyAsync()
	{
		var sut = new BufferingMemberChunkSink();

		sut.OnChunk([Substitute.For<IMember>()], chunkIndex: 0, chunkCount: 1);

		var members = await sut.Completion.WaitAsync(TimeSpan.FromSeconds(1));
		Assert.Single(members);
	}

	[Fact]
	public async Task OnCancel_FaultsCompletionAsync()
	{
		var sut = new BufferingMemberChunkSink();

		using var cts = new CancellationTokenSource();
		cts.Cancel();
		sut.OnCancel(cts.Token);

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
			sut.Completion.WaitAsync(TimeSpan.FromSeconds(1)));
	}
}

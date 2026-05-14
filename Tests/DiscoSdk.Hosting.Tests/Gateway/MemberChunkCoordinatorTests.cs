using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Models;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Gateway;

/// <summary>
/// The coordinator is policy-free — every test uses a fake <see cref="IMemberChunkSink"/> and
/// asserts on routing behavior (which sink got called, when the entry was removed, etc.).
/// </summary>
public class MemberChunkCoordinatorTests
{
	private readonly MemberChunkCoordinator _sut = new();

	[Fact]
	public void Register_ThenDeliver_ForwardsToSink()
	{
		var sink = Substitute.For<IMemberChunkSink>();
		_sut.Register("nonce-1", sink);

		var members = new[] { Substitute.For<IMember>() };
		_sut.TryDeliver("nonce-1", members, chunkIndex: 0, chunkCount: 2);

		sink.Received(1).OnChunk(members, 0, 2);
	}

	[Fact]
	public void TryDeliver_FinalChunk_RemovesEntry_AllowingReuse()
	{
		var first = Substitute.For<IMemberChunkSink>();
		_sut.Register("reuse", first);
		_sut.TryDeliver("reuse", [Substitute.For<IMember>()], chunkIndex: 0, chunkCount: 1);

		// After the final chunk the entry should be gone — re-registering must succeed.
		var second = Substitute.For<IMemberChunkSink>();
		_sut.Register("reuse", second);

		// And subsequent delivery routes to the new sink only.
		_sut.TryDeliver("reuse", [Substitute.For<IMember>()], chunkIndex: 0, chunkCount: 1);
		second.Received(1).OnChunk(Arg.Any<IReadOnlyList<IMember>>(), 0, 1);
	}

	[Fact]
	public void TryDeliver_UnknownNonce_IsIgnored()
	{
		// Should not throw.
		_sut.TryDeliver("ghost", [Substitute.For<IMember>()], chunkIndex: 0, chunkCount: 1);
	}

	[Fact]
	public void TryDeliver_EmptyOrNullNonce_IsIgnored()
	{
		var sink = Substitute.For<IMemberChunkSink>();
		_sut.Register("real", sink);

		_sut.TryDeliver(null, [Substitute.For<IMember>()], chunkIndex: 0, chunkCount: 1);
		_sut.TryDeliver(string.Empty, [Substitute.For<IMember>()], chunkIndex: 0, chunkCount: 1);

		sink.DidNotReceive().OnChunk(Arg.Any<IReadOnlyList<IMember>>(), Arg.Any<int>(), Arg.Any<int>());
	}

	[Fact]
	public void Cancel_CallsSinkOnCancel_AndRemovesEntry()
	{
		var sink = Substitute.For<IMemberChunkSink>();
		_sut.Register("to-cancel", sink);

		using var cts = new CancellationTokenSource();
		_sut.Cancel("to-cancel", cts.Token);

		sink.Received(1).OnCancel(cts.Token);

		// Entry was removed — re-registering must succeed.
		_sut.Register("to-cancel", Substitute.For<IMemberChunkSink>());
	}

	[Fact]
	public void Cancel_UnknownNonce_IsNoop()
	{
		// Should not throw and the sink (which doesn't exist) is obviously not called.
		_sut.Cancel("never-registered");
	}

	[Fact]
	public void Register_DuplicateNonce_Throws()
	{
		_sut.Register("dup", Substitute.For<IMemberChunkSink>());
		Assert.Throws<InvalidOperationException>(() => _sut.Register("dup", Substitute.For<IMemberChunkSink>()));
	}

	[Fact]
	public void Register_EmptyOrNullNonce_Throws()
	{
		// ArgumentException.ThrowIfNullOrWhiteSpace throws ArgumentNullException (subclass)
		// on null and ArgumentException on empty/whitespace.
		Assert.ThrowsAny<ArgumentException>(() => _sut.Register(string.Empty, Substitute.For<IMemberChunkSink>()));
		Assert.ThrowsAny<ArgumentException>(() => _sut.Register(null!, Substitute.For<IMemberChunkSink>()));
	}

	[Fact]
	public void Register_NullSink_Throws()
	{
		Assert.Throws<ArgumentNullException>(() => _sut.Register("nonce", null!));
	}
}

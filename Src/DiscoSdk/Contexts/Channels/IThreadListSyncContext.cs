using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using System.Collections.Immutable;

namespace DiscoSdk.Contexts.Channels;

/// <summary>
/// Context for the <c>THREAD_LIST_SYNC</c> Gateway event — sent when the client gains access to a
/// channel and Discord backfills the active threads.
/// </summary>
public interface IThreadListSyncContext : IContext
{
	/// <summary>The guild the threads belong to.</summary>
	IGuild Guild { get; }

	/// <summary>Active threads now visible to the bot.</summary>
	ImmutableArray<IGuildThreadChannel> Threads { get; }
}

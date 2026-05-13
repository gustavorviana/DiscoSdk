using DiscoSdk.Models;
using DiscoSdk.Models.Channels;

namespace DiscoSdk.Contexts.Channels;

/// <summary>
/// Context for <c>THREAD_CREATE</c> and <c>THREAD_UPDATE</c> Gateway events.
/// </summary>
public interface IThreadContext : IContext
{
	/// <summary>The thread that was created or updated.</summary>
	IGuildThreadChannel Thread { get; }

	/// <summary>The guild the thread belongs to.</summary>
	IGuild Guild { get; }
}

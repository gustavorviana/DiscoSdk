using DiscoSdk.Models;

namespace DiscoSdk.Contexts.Channels;

/// <summary>
/// Context for the <c>THREAD_DELETE</c> Gateway event — the thread itself is gone; only its
/// identifiers remain.
/// </summary>
public interface IThreadDeleteContext : IContext
{
	/// <summary>ID of the deleted thread.</summary>
	Snowflake ThreadId { get; }

	/// <summary>ID of the parent channel, if any.</summary>
	Snowflake? ParentId { get; }

	/// <summary>The guild the thread belonged to.</summary>
	IGuild Guild { get; }
}

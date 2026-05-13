using DiscoSdk.Models;
using System.Collections.Immutable;

namespace DiscoSdk.Contexts.Channels;

/// <summary>
/// Context for the <c>THREAD_MEMBERS_UPDATE</c> Gateway event — users joined or left a thread.
/// </summary>
public interface IThreadMembersUpdateContext : IContext
{
	/// <summary>ID of the affected thread.</summary>
	Snowflake ThreadId { get; }

	/// <summary>The guild containing the thread.</summary>
	IGuild Guild { get; }

	/// <summary>Approximate member count after the change (capped by Discord at 50).</summary>
	int MemberCount { get; }

	/// <summary>User IDs that joined the thread in this event.</summary>
	ImmutableArray<Snowflake> AddedUserIds { get; }

	/// <summary>User IDs that left or were removed from the thread.</summary>
	ImmutableArray<Snowflake> RemovedUserIds { get; }
}

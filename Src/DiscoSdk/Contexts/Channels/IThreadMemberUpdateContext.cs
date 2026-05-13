using DiscoSdk.Models;

namespace DiscoSdk.Contexts.Channels;

/// <summary>
/// Context for the <c>THREAD_MEMBER_UPDATE</c> Gateway event — the bot's own thread-member object
/// changed (e.g., flags update).
/// </summary>
public interface IThreadMemberUpdateContext : IContext
{
	/// <summary>The thread the member belongs to.</summary>
	Snowflake ThreadId { get; }

	/// <summary>The user whose thread-member state changed (typically the bot itself).</summary>
	Snowflake UserId { get; }

	/// <summary>The guild containing the thread.</summary>
	IGuild Guild { get; }
}

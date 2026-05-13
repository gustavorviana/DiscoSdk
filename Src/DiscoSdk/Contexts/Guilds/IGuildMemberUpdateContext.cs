using DiscoSdk.Models;

namespace DiscoSdk.Contexts.Guilds;

/// <summary>
/// Context for the <c>GUILD_MEMBER_UPDATE</c> Gateway event — a guild member's attributes
/// (nickname, roles, timeout, boost state, etc.) changed.
/// </summary>
public interface IGuildMemberUpdateContext : IContext
{
	/// <summary>The member with its updated state.</summary>
	IMember Member { get; }

	/// <summary>The guild the member belongs to.</summary>
	IGuild Guild { get; }
}

using DiscoSdk.Models;

namespace DiscoSdk.Contexts.Guilds;

/// <summary>
/// Context for the <c>GUILD_MEMBER_ADD</c> Gateway event — a user joined a guild.
/// </summary>
public interface IGuildMemberAddContext : IContext
{
	/// <summary>The newly joined member.</summary>
	IMember Member { get; }

	/// <summary>The guild the member joined.</summary>
	IGuild Guild { get; }
}

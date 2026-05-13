using DiscoSdk.Models;

namespace DiscoSdk.Contexts.Guilds;

/// <summary>
/// Context for <c>GUILD_ROLE_CREATE</c> and <c>GUILD_ROLE_UPDATE</c> Gateway events.
/// </summary>
public interface IGuildRoleContext : IContext
{
	/// <summary>The role that was created or updated.</summary>
	IRole Role { get; }

	/// <summary>The guild the role belongs to.</summary>
	IGuild Guild { get; }
}

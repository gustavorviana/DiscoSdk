using DiscoSdk.Models;

namespace DiscoSdk.Contexts.Guilds;

/// <summary>
/// Context for the <c>GUILD_ROLE_DELETE</c> Gateway event. The role object itself is no longer
/// available; only its ID and the guild remain.
/// </summary>
public interface IGuildRoleDeleteContext : IContext
{
	/// <summary>The ID of the deleted role.</summary>
	Snowflake RoleId { get; }

	/// <summary>The guild the role belonged to.</summary>
	IGuild Guild { get; }
}

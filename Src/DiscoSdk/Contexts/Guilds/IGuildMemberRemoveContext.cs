using DiscoSdk.Models;

namespace DiscoSdk.Contexts.Guilds;

/// <summary>
/// Context for the <c>GUILD_MEMBER_REMOVE</c> Gateway event — a user left, was kicked, or was banned
/// from a guild. The full <see cref="IMember"/> is no longer available; only the user identity remains.
/// </summary>
public interface IGuildMemberRemoveContext : IContext
{
	/// <summary>The user who left/was removed.</summary>
	IUser User { get; }

	/// <summary>The guild the user was removed from.</summary>
	IGuild Guild { get; }
}

using DiscoSdk.Models;

namespace DiscoSdk.Contexts.Guilds;

/// <summary>
/// Context for <c>GUILD_BAN_ADD</c> and <c>GUILD_BAN_REMOVE</c> Gateway events.
/// </summary>
public interface IGuildBanContext : IContext
{
	/// <summary>The user that was banned/unbanned.</summary>
	IUser User { get; }

	/// <summary>The guild in which the ban changed.</summary>
	IGuild Guild { get; }
}

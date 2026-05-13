using DiscoSdk.Models;
using DiscoSdk.Models.Presences;

namespace DiscoSdk.Contexts.Guilds;

/// <summary>
/// Context for the <c>PRESENCE_UPDATE</c> Gateway event — a user's presence (online status, activity,
/// client status) changed. Requires the <c>GuildPresences</c> intent.
/// </summary>
public interface IPresenceUpdateContext : IContext
{
	/// <summary>The presence payload.</summary>
	Presence Presence { get; }

	/// <summary>The guild the presence applies to, or null if not available.</summary>
	IGuild? Guild { get; }
}

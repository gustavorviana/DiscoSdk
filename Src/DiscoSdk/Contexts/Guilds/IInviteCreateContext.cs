using DiscoSdk.Models;
using DiscoSdk.Models.Channels;

namespace DiscoSdk.Contexts.Guilds;

/// <summary>
/// Context for the <c>INVITE_CREATE</c> Gateway event.
/// </summary>
public interface IInviteCreateContext : IContext
{
	/// <summary>The unique invite code.</summary>
	string Code { get; }

	/// <summary>The channel the invite targets.</summary>
	ITextBasedChannel Channel { get; }

	/// <summary>The guild the invite belongs to, or null for group DM invites.</summary>
	IGuild? Guild { get; }

	/// <summary>The user who created the invite, if known.</summary>
	IUser? Inviter { get; }

	/// <summary>When the invite was created.</summary>
	DateTimeOffset CreatedAt { get; }

	/// <summary>Duration (seconds) before the invite expires. 0 = never.</summary>
	int MaxAge { get; }

	/// <summary>Maximum number of times this invite can be used. 0 = unlimited.</summary>
	int MaxUses { get; }

	/// <summary>Whether this invite grants temporary membership.</summary>
	bool Temporary { get; }
}

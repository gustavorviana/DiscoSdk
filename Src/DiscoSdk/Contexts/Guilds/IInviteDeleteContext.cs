using DiscoSdk.Models;
using DiscoSdk.Models.Channels;

namespace DiscoSdk.Contexts.Guilds;

/// <summary>
/// Context for the <c>INVITE_DELETE</c> Gateway event.
/// </summary>
public interface IInviteDeleteContext : IContext
{
	/// <summary>The unique invite code that was deleted.</summary>
	string Code { get; }

	/// <summary>The channel the invite was for.</summary>
	ITextBasedChannel Channel { get; }

	/// <summary>The guild the invite belonged to, or null for group DM invites.</summary>
	IGuild? Guild { get; }
}

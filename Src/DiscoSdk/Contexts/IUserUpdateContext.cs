using DiscoSdk.Models;

namespace DiscoSdk.Contexts;

/// <summary>
/// Context for the <c>USER_UPDATE</c> Gateway event — the bot's own user object changed (username,
/// avatar, etc.).
/// </summary>
public interface IUserUpdateContext : IContext
{
	/// <summary>The updated user (the bot itself).</summary>
	IUser User { get; }
}

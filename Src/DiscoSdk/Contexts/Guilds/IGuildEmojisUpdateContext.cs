using DiscoSdk.Models;
using System.Collections.Immutable;

namespace DiscoSdk.Contexts.Guilds;

/// <summary>
/// Context for the <c>GUILD_EMOJIS_UPDATE</c> Gateway event — the guild's emoji set changed.
/// </summary>
public interface IGuildEmojisUpdateContext : IContext
{
	/// <summary>The guild whose emojis changed.</summary>
	IGuild Guild { get; }

	/// <summary>The new emoji set.</summary>
	ImmutableArray<IEmoji> Emojis { get; }
}

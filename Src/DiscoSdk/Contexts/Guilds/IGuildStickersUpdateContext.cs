using DiscoSdk.Models;
using System.Collections.Immutable;

namespace DiscoSdk.Contexts.Guilds;

/// <summary>
/// Context for the <c>GUILD_STICKERS_UPDATE</c> Gateway event — the guild's sticker set changed.
/// </summary>
public interface IGuildStickersUpdateContext : IContext
{
	/// <summary>The guild whose stickers changed.</summary>
	IGuild Guild { get; }

	/// <summary>The new sticker set.</summary>
	ImmutableArray<ISticker> Stickers { get; }
}

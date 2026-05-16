using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using System.Collections.Immutable;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class GuildEmojisUpdateContextWrapper(DiscordClient client, IGuild guild, ImmutableArray<InternalEmoji> emojis)
	: ContextWrapper(client), IGuildEmojisUpdateContext
{
	private ImmutableArray<IEmoji>? _wrapped;
	public IGuild Guild => guild;
	public ImmutableArray<IEmoji> Emojis => _wrapped ??= [.. emojis.Select(e => new EmojiWrapper(client, e, guild))];
}

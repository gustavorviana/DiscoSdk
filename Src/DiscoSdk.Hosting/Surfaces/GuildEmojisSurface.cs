using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Surfaces;

internal sealed class GuildEmojisSurface(DiscordClient client, GuildWrapper guild) : IGuildEmojis
{
    private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly GuildWrapper _guild = guild ?? throw new ArgumentNullException(nameof(guild));

    public ICreateEmojiAction Create(string name, DiscordImageBuffer image)
        => new CreateEmojiAction(_client, _guild, name, image);

    public IReadOnlyList<IEmoji> GetCached() => _guild.CachedEmojisSnapshot;

    public int GetCachedCount() => _guild.CachedEmojisSnapshot.Length;
}

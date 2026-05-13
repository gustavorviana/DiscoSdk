using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers.Channels;

/// <summary>
/// Wrapper that implements <see cref="IGuildCategoryChannel"/> for a <see cref="Channel"/> instance.
/// </summary>
internal class GuildCategoryChannelWrapper(DiscordClient client, Channel channel, IGuild guild)
    : GuildChannelWrapperBase(client, channel, guild), IGuildCategoryChannel
{
    public IRestAction<IReadOnlyList<IGuildChannel>> GetChannels()
    {
        return RestAction<IReadOnlyList<IGuildChannel>>.Create(async cancellationToken =>
        {
            var channels = await _client.GuildClient.GetChannelsAsync(Guild.Id, cancellationToken);
            return channels
                .Where(c => c.ParentId == Id)
                .Select(c => ToSpecificType(_client, c, Guild))
                .OfType<IGuildChannel>()
                .ToList()
                .AsReadOnly();
        });
    }
}

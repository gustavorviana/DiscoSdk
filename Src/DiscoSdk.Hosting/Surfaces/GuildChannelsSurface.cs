using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Hosting.Wrappers.Channels;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;
using DiscoSdk.Utils;

namespace DiscoSdk.Hosting.Surfaces;

/// <summary>
/// Concrete implementation of <see cref="IGuildChannels"/>. Reads delegate to the in-memory channel
/// snapshot held by <see cref="GuildWrapper"/>; mutations build deferred REST actions.
/// </summary>
internal sealed class GuildChannelsSurface(DiscordClient client, GuildWrapper guild) : IGuildChannels
{
    private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly GuildWrapper _guild = guild ?? throw new ArgumentNullException(nameof(guild));

    public Snowflake? AfkId => _guild.Data.AfkChannelId;
    public int? AfkTimeout => _guild.Data.AfkTimeout;
    public Snowflake? SystemId => _guild.Data.SystemChannelId;
    public SystemChannelFlags? SystemFlags => _guild.Data.SystemChannelFlags;
    public Snowflake? RulesId => _guild.Data.RulesChannelId;
    public Snowflake? PublicUpdatesId => _guild.Data.PublicUpdatesChannelId;

    public ICreateChannelAction Create(string name, ChannelType type)
        => new CreateChannelAction(_client, _guild, name, type);

    public IGuildChannelUnion? Get(Snowflake channelId)
    {
        var channel = _guild.RawChannelById(channelId);
        return channel is null ? null : new GuildChannelUnionWrapper(_client, channel, _guild);
    }

    public IReadOnlyList<IGuildChannelUnion> GetAll()
        => [.. _guild.ChannelsSnapshot().Select(ch => new GuildChannelUnionWrapper(_client, ch, _guild))];

    public IReadOnlyList<IGuildTextChannel> GetText()
        => [.. _guild.ChannelsSnapshot()
            .Where(x => ChannelTypeUtils.IsText(x.Type))
            .Select(ch => ChannelWrapper.ToSpecificType(_client, ch, _guild))
            .OfType<IGuildTextChannel>()];

    public IReadOnlyList<IGuildVoiceChannel> GetVoice()
        => [.. _guild.ChannelsSnapshot()
            .Where(x => ChannelTypeUtils.IsVoice(x.Type))
            .Select(ch => new GuildVoiceChannelWrapper(_client, ch, _guild))];

    public IGuildVoiceChannel? GetAfk()
    {
        if (!AfkId.HasValue) return null;
        var channel = _guild.RawChannelById(AfkId.Value);
        return channel is null ? null : new GuildVoiceChannelWrapper(_client, channel, _guild);
    }

    public IGuildTextChannel? GetSystem() => GetTextChannelById(SystemId);
    public IGuildTextChannel? GetRules() => GetTextChannelById(RulesId);
    public IGuildTextChannel? GetPublicUpdates() => GetTextChannelById(PublicUpdatesId);

    public IReasonedRestAction ModifyPositions(IEnumerable<ChannelPosition> positions)
    {
        ArgumentNullException.ThrowIfNull(positions);
        var gid = _guild.Id;
        return new ReasonedRestAction((reason, ct) =>
        {
            var payload = positions.Select(p => new Dictionary<string, object?>
            {
                ["id"] = p.Id.ToString(),
                ["position"] = p.Position,
                ["lock_permissions"] = p.LockPermissions,
                ["parent_id"] = p.ParentId.HasValue ? p.ParentId.Value.ToString() : null,
            });
            return _client.GuildClient.ModifyChannelPositionsAsync(gid, payload, reason, ct);
        });
    }

    public IRestAction<IReadOnlyList<IGuildThreadChannel>> ListActiveThreads()
        => RestAction<IReadOnlyList<IGuildThreadChannel>>.Create(async ct =>
        {
            var threads = await _client.GuildClient.ListActiveThreadsAsync(_guild.Id, ct).ConfigureAwait(false);
            return threads
                .Select(c => ChannelWrapper.ToSpecificType(_client, c, _guild))
                .OfType<IGuildThreadChannel>()
                .ToList()
                .AsReadOnly();
        });

    private IGuildTextChannel? GetTextChannelById(Snowflake? channelId)
    {
        var channel = _guild.RawChannelById(channelId);
        if (channel is null) return null;
        if (!ChannelTypeUtils.IsText(channel.Type)) throw new InvalidCastException();
        return new GuildTextChannelWrapper(_client, channel, _guild);
    }
}

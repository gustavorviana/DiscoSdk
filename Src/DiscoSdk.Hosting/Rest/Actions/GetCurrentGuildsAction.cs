using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

internal sealed class GetCurrentGuildsAction : RestAction<IReadOnlyList<IGuild>>, IGetCurrentGuildsAction
{
    private readonly DiscordClient _client;
    private int? _limit;
    private Snowflake? _before;
    private Snowflake? _after;
    private bool? _withCounts;

    public GetCurrentGuildsAction(DiscordClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public IGetCurrentGuildsAction SetLimit(int limit)
    {
        if (limit is < 1 or > 200)
            throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 200.");
        _limit = limit;
        return this;
    }

    public IGetCurrentGuildsAction Before(Snowflake guildId) { _before = guildId; return this; }
    public IGetCurrentGuildsAction After(Snowflake guildId) { _after = guildId; return this; }
    public IGetCurrentGuildsAction WithCounts() { _withCounts = true; return this; }

    public override async Task<IReadOnlyList<IGuild>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var guilds = await _client.UserClient.GetCurrentGuildsAsync(_limit, _before, _after, _withCounts, cancellationToken);
        return guilds.Select(g => (IGuild)new GuildWrapper(g, _client)).ToList().AsReadOnly();
    }
}

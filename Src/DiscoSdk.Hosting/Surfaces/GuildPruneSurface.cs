using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Surfaces;

internal sealed class GuildPruneSurface(DiscordClient client, Snowflake guildId) : IGuildPrune
{
    private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));

    public IRestAction<int> Count(int days, params Snowflake[] includeRoles)
        => RestAction<int>.Create(ct =>
            _client.GuildClient.GetPruneCountAsync(guildId, days, includeRoles, ct));

    public IReasonedRestAction<int> Begin(int days, params Snowflake[] includeRoles)
        => new ReasonedRestAction<int>((reason, ct) =>
            _client.GuildClient.BeginPruneAsync(guildId, days, includeRoles, reason, ct));
}

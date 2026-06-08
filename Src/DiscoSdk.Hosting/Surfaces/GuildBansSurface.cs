using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;
using System.Text.Json;

namespace DiscoSdk.Hosting.Surfaces;

/// <summary>
/// Concrete implementation of <see cref="IGuildBans"/>. REST-only — there is no ban cache, so this
/// is a thin builder factory pre-bound to a guild id.
/// </summary>
internal sealed class GuildBansSurface(DiscordClient client, Snowflake guildId) : IGuildBans
{
    private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));

    public IRestAction<IBan?> Get(Snowflake userId)
        => RestAction<IBan?>.Create(async ct =>
        {
            var ban = await _client.GuildClient.GetBanAsync(guildId, userId, ct).ConfigureAwait(false);
            return ban is null ? null : new BanWrapper(ban, _client);
        });

    public IBanPaginationAction List()
        => new BanPaginationAction(_client, guildId);

    public IBanMemberAction Ban(Snowflake userId, int deleteMessageDays = 0)
        => new BanMemberAction(_client, guildId, userId, deleteMessageDays);

    public IReasonedRestAction Unban(Snowflake userId)
        => new ReasonedRestAction((reason, ct) =>
            _client.GuildClient.UnbanMemberAsync(guildId, userId, reason, ct));

    public IReasonedRestAction<IReadOnlyList<Snowflake>> BulkBan(IEnumerable<Snowflake> userIds, int? deleteMessageSeconds = null)
    {
        ArgumentNullException.ThrowIfNull(userIds);
        return new ReasonedRestAction<IReadOnlyList<Snowflake>>(async (reason, ct) =>
        {
            var response = await _client.GuildClient.BulkBanAsync(guildId, userIds, deleteMessageSeconds, reason, ct).ConfigureAwait(false);
            var banned = new List<Snowflake>();
            if (response.ValueKind == JsonValueKind.Object &&
                response.TryGetProperty("banned_users", out var bannedArr) &&
                bannedArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in bannedArr.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String &&
                        Snowflake.TryParse(item.GetString()!, out var id))
                        banned.Add(id);
                }
            }
            return banned.AsReadOnly();
        });
    }
}

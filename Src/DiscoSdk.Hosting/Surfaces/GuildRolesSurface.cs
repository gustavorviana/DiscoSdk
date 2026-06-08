using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Surfaces;

internal sealed class GuildRolesSurface(DiscordClient client, GuildWrapper guild) : IGuildRoles
{
    private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly GuildWrapper _guild = guild ?? throw new ArgumentNullException(nameof(guild));

    public IRoleAction Create() => new RoleAction(_client, _guild);

    public IRestAction<IReadOnlyList<IRole>> GetAll()
        => RestAction<IReadOnlyList<IRole>>.Create(async ct =>
        {
            var roles = await _client.GuildClient.GetRolesAsync(_guild.Id, ct).ConfigureAwait(false);
            return roles
                .Select(r => new RoleWrapper(_client, r, _guild))
                .Cast<IRole>()
                .ToList()
                .AsReadOnly();
        });

    public IRestAction<IRole?> Get(Snowflake roleId)
    {
        if (roleId == default)
            throw new ArgumentException("Role ID cannot be null or empty.", nameof(roleId));

        return RestAction<IRole?>.Create(async ct =>
        {
            var role = await _client.RoleClient.GetAsync(_guild.Id, roleId, ct).ConfigureAwait(false);
            return role is null ? null : new RoleWrapper(_client, role, _guild);
        });
    }

    public IModifyRolePositionsAction ModifyPositions()
        => new ModifyRolePositionsAction(_client, _guild);

    public IReadOnlyList<IRole> GetCached() => _guild.CachedRolesSnapshot;

    public int GetCachedCount() => _guild.CachedRolesSnapshot.Length;
}

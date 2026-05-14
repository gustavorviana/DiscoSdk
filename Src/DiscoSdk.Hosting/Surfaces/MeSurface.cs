using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Applications;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Surfaces;

/// <summary>
/// Concrete implementation of <see cref="IMe"/>. Delegates to <see cref="UserClient"/> for the
/// <c>@me</c> namespace and wraps responses with the existing entity wrappers.
/// </summary>
internal sealed class MeSurface(DiscordClient client) : IMe
{
    private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));

    /// <inheritdoc />
    public IRestAction<IUser> Get()
        => RestAction<IUser>.Create(async ct => new UserWrapper(_client, await _client.UserClient.GetCurrentAsync(ct)));

    /// <inheritdoc />
    public IRestAction<IUser> Modify(string? username = null, string? avatar = null, string? banner = null)
        => RestAction<IUser>.Create(async ct =>
        {
            var body = new Dictionary<string, object?>();
            if (username is not null) body["username"] = username;
            if (avatar is not null) body["avatar"] = avatar.Length == 0 ? null : avatar;
            if (banner is not null) body["banner"] = banner.Length == 0 ? null : banner;
            var user = await _client.UserClient.ModifyCurrentAsync(body, ct);
            return new UserWrapper(_client, user);
        });

    /// <inheritdoc />
    public IRestAction<IReadOnlyList<IGuild>> GetGuilds(int? limit = null, Snowflake? before = null, Snowflake? after = null, bool? withCounts = null)
        => RestAction<IReadOnlyList<IGuild>>.Create(async ct =>
        {
            var guilds = await _client.UserClient.GetCurrentGuildsAsync(limit, before, after, withCounts, ct);
            return guilds.Select(g => (IGuild)new GuildWrapper(g, _client)).ToList().AsReadOnly();
        });

    /// <inheritdoc />
    public IRestAction<IMember> GetGuildMember(Snowflake guildId)
        => RestAction<IMember>.Create(async ct =>
        {
            var member = await _client.UserClient.GetCurrentGuildMemberAsync(guildId, ct);
            var guild = await _client.Guilds.GetAsync(guildId, ct)
                ?? throw new InvalidOperationException($"Guild {guildId} not found.");
            return new GuildMemberWrapper(_client, member, guild);
        });

    /// <inheritdoc />
    public IRestAction<IReadOnlyList<IConnection>> GetConnections()
        => RestAction<IReadOnlyList<IConnection>>.Create(async ct =>
        {
            var connections = await _client.UserClient.GetConnectionsAsync(ct);
            return connections.Cast<IConnection>().ToList().AsReadOnly();
        });

    /// <inheritdoc />
    public IRestAction<IApplicationRoleConnection> GetApplicationRoleConnection(Snowflake applicationId)
        => RestAction<IApplicationRoleConnection>.Create(async ct => await _client.UserClient.GetApplicationRoleConnectionAsync(applicationId, ct));

    /// <inheritdoc />
    public IRestAction<IApplicationRoleConnection> UpdateApplicationRoleConnection(Snowflake applicationId, ApplicationRoleConnection record)
    {
        ArgumentNullException.ThrowIfNull(record);
        return RestAction<IApplicationRoleConnection>.Create(async ct => await _client.UserClient.UpdateApplicationRoleConnectionAsync(applicationId, record, ct));
    }
}

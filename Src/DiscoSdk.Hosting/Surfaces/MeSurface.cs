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
    public IModifyMeAction Modify() => new ModifyMeAction(_client);

    /// <inheritdoc />
    public IGetCurrentGuildsAction GetGuilds() => new GetCurrentGuildsAction(_client);

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

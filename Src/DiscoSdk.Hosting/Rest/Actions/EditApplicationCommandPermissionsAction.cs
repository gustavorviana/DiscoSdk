using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Concrete <see cref="IEditApplicationCommandPermissionsAction"/>. Accumulates overrides in an
/// internal list and on <see cref="ExecuteAsync"/> posts them as the full replacement set. The
/// bearer token supplied via <see cref="WithBearerToken"/> is threaded through to the REST
/// client's per-request <c>Authorization</c> override — the bot token never leaves the bot path.
/// </summary>
internal sealed class EditApplicationCommandPermissionsAction(DiscordClient client, Snowflake guildId, Snowflake commandId) : RestAction<IApplicationCommandPermissions>, IEditApplicationCommandPermissionsAction
{
    private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly List<ApplicationCommandPermission> _permissions = [];
    private string? _bearerToken;

    /// <inheritdoc />
    public IEditApplicationCommandPermissionsAction WithBearerToken(string accessToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);
        _bearerToken = accessToken;
        return this;
    }

    /// <inheritdoc />
    public IEditApplicationCommandPermissionsAction AllowRole(Snowflake roleId) => Add(ApplicationCommandPermissionType.Role, roleId, true);

    /// <inheritdoc />
    public IEditApplicationCommandPermissionsAction DenyRole(Snowflake roleId) => Add(ApplicationCommandPermissionType.Role, roleId, false);

    /// <inheritdoc />
    public IEditApplicationCommandPermissionsAction AllowUser(Snowflake userId) => Add(ApplicationCommandPermissionType.User, userId, true);

    /// <inheritdoc />
    public IEditApplicationCommandPermissionsAction DenyUser(Snowflake userId) => Add(ApplicationCommandPermissionType.User, userId, false);

    /// <inheritdoc />
    public IEditApplicationCommandPermissionsAction AllowChannel(Snowflake channelId) => Add(ApplicationCommandPermissionType.Channel, channelId, true);

    /// <inheritdoc />
    public IEditApplicationCommandPermissionsAction DenyChannel(Snowflake channelId) => Add(ApplicationCommandPermissionType.Channel, channelId, false);

    /// <inheritdoc />
    public IEditApplicationCommandPermissionsAction Add(ApplicationCommandPermissionType type, Snowflake id, bool allowed)
    {
        _permissions.Add(new ApplicationCommandPermission { Id = id, Type = type, Permission = allowed });
        return this;
    }

    /// <inheritdoc />
    public IEditApplicationCommandPermissionsAction Clear()
    {
        _permissions.Clear();
        return this;
    }

    /// <inheritdoc />
    public override async Task<IApplicationCommandPermissions> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_bearerToken))
            throw new InvalidOperationException(
                "EditCommandPermissions requires a user OAuth2 bearer token with the " +
                "applications.commands.permissions.update scope. Call WithBearerToken(...) before ExecuteAsync. " +
                "Discord rejects bot-token requests against this endpoint with 401.");

        var response = await _client.ApplicationCommandClient.EditCommandPermissionsAsync(
            _client.RequireApplicationId(),
            guildId,
            commandId,
            _permissions.ToArray(),
            _bearerToken,
            cancellationToken);

        return new ApplicationCommandPermissionsWrapper(response);
    }
}

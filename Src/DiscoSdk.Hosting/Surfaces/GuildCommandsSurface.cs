using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Surfaces;

/// <summary>
/// Concrete implementation of <see cref="IGuildCommands"/>. Bound to a specific guild ID; the
/// owning <see cref="GuildWrapper"/> instantiates one per guild and exposes it via
/// <see cref="IGuild.Commands"/>.
/// </summary>
internal sealed class GuildCommandsSurface(DiscordClient client, Snowflake guildId) : IGuildCommands
{
    private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));

    /// <inheritdoc />
    public IRestAction<IReadOnlyList<IApplicationCommandPermissions>> GetAllPermissions()
        => RestAction<IReadOnlyList<IApplicationCommandPermissions>>.Create(async ct =>
        {
            var permissions = await _client.ApplicationCommandClient.GetGuildCommandsPermissionsAsync(_client.RequireApplicationId(), guildId, ct);
            return permissions.Select(p => (IApplicationCommandPermissions)new ApplicationCommandPermissionsWrapper(p)).ToList().AsReadOnly();
        });

    /// <inheritdoc />
    public IRestAction<IApplicationCommandPermissions> GetPermissions(Snowflake commandId)
        => RestAction<IApplicationCommandPermissions>.Create(async ct =>
            new ApplicationCommandPermissionsWrapper(await _client.ApplicationCommandClient.GetCommandPermissionsAsync(_client.RequireApplicationId(), guildId, commandId, ct)));

    /// <inheritdoc />
    public IEditApplicationCommandPermissionsAction EditPermissions(Snowflake commandId)
        => new EditApplicationCommandPermissionsAction(_client, guildId, commandId);
}

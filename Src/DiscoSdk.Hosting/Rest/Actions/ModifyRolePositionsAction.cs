using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

internal sealed class ModifyRolePositionsAction(DiscordClient client, IGuild guild)
    : RestAction<IReadOnlyList<IRole>>, IModifyRolePositionsAction
{
    private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly IGuild _guild = guild ?? throw new ArgumentNullException(nameof(guild));
    private readonly Dictionary<Snowflake, (int? Position, bool? LockPermissions)> _moves = [];
    private string? _reason;

    public IModifyRolePositionsAction Move(Snowflake roleId, int? position, bool? lockPermissions = null)
    {
        if (roleId == default)
            throw new ArgumentException("Role ID cannot be null or empty.", nameof(roleId));

        _moves[roleId] = (position, lockPermissions);
        return this;
    }

    public IModifyRolePositionsAction WithReason(string reason)
    {
        _reason = AuditLogReason.Validate(reason);
        return this;
    }

    public override async Task<IReadOnlyList<IRole>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (_moves.Count == 0)
            throw new InvalidOperationException("ModifyRolePositions requires at least one Move call before ExecuteAsync.");

        var payload = _moves
            .Select(kv =>
            {
                var entry = new Dictionary<string, object?> { ["id"] = kv.Key.ToString() };
                if (kv.Value.Position.HasValue)
                    entry["position"] = kv.Value.Position.Value;
                if (kv.Value.LockPermissions.HasValue)
                    entry["lock_permissions"] = kv.Value.LockPermissions.Value;
                return entry;
            })
            .ToArray();

        var roles = await _client.RoleClient.ModifyPositionsAsync(_guild.Id, payload, _reason, cancellationToken);
        return roles.Select(r => (IRole)new RoleWrapper(_client, r, _guild)).ToList().AsReadOnly();
    }
}

using DiscoSdk.Models;
using DiscoSdk.Models.Commands;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Wraps an <see cref="ApplicationCommandPermissions"/> model. No actions on this surface —
/// edits go through <see cref="IGuild.EditCommandPermissions"/>'s fluent builder, not the
/// read-side wrapper.
/// </summary>
internal sealed class ApplicationCommandPermissionsWrapper(ApplicationCommandPermissions model) : IApplicationCommandPermissions
{
    private readonly ApplicationCommandPermissions _model = model ?? throw new ArgumentNullException(nameof(model));
    private IReadOnlyList<IApplicationCommandPermission>? _permissions;

    /// <inheritdoc />
    public Snowflake Id => _model.Id;

    /// <inheritdoc />
    public Snowflake ApplicationId => _model.ApplicationId;

    /// <inheritdoc />
    public Snowflake GuildId => _model.GuildId;

    /// <inheritdoc />
    public IReadOnlyList<IApplicationCommandPermission> Permissions
        => _permissions ??= _model.Permissions
            .Select(p => (IApplicationCommandPermission)new ApplicationCommandPermissionWrapper(p))
            .ToList()
            .AsReadOnly();
}

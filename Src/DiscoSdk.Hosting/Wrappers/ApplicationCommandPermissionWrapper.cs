using DiscoSdk.Models;
using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Wraps a single <see cref="ApplicationCommandPermission"/> entry. Renames the wire field
/// <c>permission</c> (a verb-less noun that flips between "allow" and "deny") to
/// <see cref="Allowed"/> on the public surface.
/// </summary>
internal sealed class ApplicationCommandPermissionWrapper(ApplicationCommandPermission model) : IApplicationCommandPermission
{
    private readonly ApplicationCommandPermission _model = model ?? throw new ArgumentNullException(nameof(model));

    /// <inheritdoc />
    public Snowflake Id => _model.Id;

    /// <inheritdoc />
    public ApplicationCommandPermissionType Type => _model.Type;

    /// <inheritdoc />
    public bool Allowed => _model.Permission;
}

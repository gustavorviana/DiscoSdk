using DiscoSdk.Models.Enums;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a permission override for a channel, wrapping a <see cref="PermissionOverwrite"/>.
/// </summary>
public class PermissionOverride
{
	private readonly PermissionOverwrite _overwrite;

	/// <summary>
	/// Initializes a new instance of the <see cref="PermissionOverride"/> class.
	/// </summary>
	/// <param name="overwrite">The permission overwrite to wrap.</param>
	public PermissionOverride(PermissionOverwrite overwrite)
	{
		_overwrite = overwrite ?? throw new ArgumentNullException(nameof(overwrite));
	}

	/// <summary>
	/// Gets the underlying permission overwrite.
	/// </summary>
	public PermissionOverwrite Overwrite => _overwrite;

	/// <summary>
	/// Gets the ID of the role or member this override applies to.
	/// </summary>
	public DiscordId Id => _overwrite.Id;

	/// <summary>
	/// Gets the type of this permission override.
	/// </summary>
	public PermissionOverwriteType Type => _overwrite.Type;

	/// <summary>
	/// Gets a value indicating whether this is a member override.
	/// </summary>
	public bool IsMemberOverride => _overwrite.Type == PermissionOverwriteType.Member;

	/// <summary>
	/// Gets a value indicating whether this is a role override.
	/// </summary>
	public bool IsRoleOverride => _overwrite.Type == PermissionOverwriteType.Role;
}


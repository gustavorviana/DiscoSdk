namespace DiscoSdk.Models;

/// <summary>
/// Represents the colors of a Discord role.
/// </summary>
public class RoleColors
{
	private readonly Color _primary;

	/// <summary>
	/// Initializes a new instance of the <see cref="RoleColors"/> class.
	/// </summary>
	/// <param name="primary">The primary color of the role.</param>
	public RoleColors(Color primary)
	{
		_primary = primary;
	}

	/// <summary>
	/// Gets the primary color of the role.
	/// </summary>
	public Color Primary => _primary;

	/// <summary>
	/// Gets the primary color as a raw integer value.
	/// </summary>
	public int PrimaryRaw => _primary.Value;
}


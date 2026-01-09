namespace DiscoSdk.Models;

/// <summary>
/// Represents an icon for a Discord role.
/// </summary>
public class RoleIcon
{
	private readonly string? _iconHash;
	private readonly string? _emoji;

	/// <summary>
	/// Initializes a new instance of the <see cref="RoleIcon"/> class with an icon hash.
	/// </summary>
	/// <param name="iconHash">The icon hash.</param>
	public RoleIcon(string iconHash)
	{
		_iconHash = iconHash ?? throw new ArgumentNullException(nameof(iconHash));
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="RoleIcon"/> class with an emoji.
	/// </summary>
	/// <param name="emoji">The emoji string.</param>
	/// <param name="isEmoji">Indicates this is an emoji (not an icon hash).</param>
	public RoleIcon(string emoji, bool isEmoji)
	{
		if (isEmoji)
			_emoji = emoji ?? throw new ArgumentNullException(nameof(emoji));
		else
			_iconHash = emoji ?? throw new ArgumentNullException(nameof(emoji));
	}

	/// <summary>
	/// Gets the icon hash, or null if this is an emoji icon.
	/// </summary>
	public string? IconHash => _iconHash;

	/// <summary>
	/// Gets the emoji, or null if this is an icon hash.
	/// </summary>
	public string? Emoji => _emoji;

	/// <summary>
	/// Gets a value indicating whether this icon is an emoji.
	/// </summary>
	public bool IsEmoji => _emoji != null;

	/// <summary>
	/// Gets a value indicating whether this icon is an icon hash.
	/// </summary>
	public bool IsIconHash => _iconHash != null;
}


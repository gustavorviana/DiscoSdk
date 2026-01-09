namespace DiscoSdk.Models;

/// <summary>
/// Represents a Discord user.
/// </summary>
public interface IUser : IMentionable
{
	/// <summary>
	/// Gets the effective avatar URL of this user.
	/// </summary>
	string EffectiveAvatarUrl { get; }

	/// <summary>
	/// Gets the effective avatar of this user.
	/// </summary>
	ImageProxy EffectiveAvatar { get; }
}


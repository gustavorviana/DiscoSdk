using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a Discord user.
/// </summary>
public interface IUser : IMentionable
{
	/// <summary>
	/// Gets the user's username.
	/// </summary>
	string Username { get; }

	/// <summary>
	/// Gets the user's discriminator.
	/// </summary>
	string Discriminator { get; }

	/// <summary>
	/// Gets the user's global display name.
	/// </summary>
	string? GlobalName { get; }

	/// <summary>
	/// Gets the user's avatar hash.
	/// </summary>
	string? Avatar { get; }

	/// <summary>
	/// Gets a value indicating whether the user is a bot.
	/// </summary>
	bool Bot { get; }

	/// <summary>
	/// Gets a value indicating whether the user is a system user.
	/// </summary>
	bool System { get; }

	/// <summary>
	/// Gets a value indicating whether the user has two-factor authentication enabled.
	/// </summary>
	bool MfaEnabled { get; }

	/// <summary>
	/// Gets the user's banner hash.
	/// </summary>
	string? Banner { get; }

	/// <summary>
	/// Gets the user's accent color.
	/// </summary>
	int? AccentColor { get; }

	/// <summary>
	/// Gets the user's locale.
	/// </summary>
	string? Locale { get; }

	/// <summary>
	/// Gets a value indicating whether the user's email is verified.
	/// </summary>
	bool Verified { get; }

	/// <summary>
	/// Gets the user's email address.
	/// </summary>
	string? Email { get; }

	/// <summary>
	/// Gets the user's flags.
	/// </summary>
	UserFlags Flags { get; }

	/// <summary>
	/// Gets the user's premium type.
	/// </summary>
	PremiumType PremiumType { get; }

	/// <summary>
	/// Gets the user's public flags.
	/// </summary>
	UserFlags PublicFlags { get; }

	/// <summary>
	/// Gets the effective avatar URL of this user.
	/// </summary>
	string EffectiveAvatarUrl { get; }

	/// <summary>
	/// Gets the effective banner URL of this user.
	/// </summary>
	string? EffectiveBannerUrl { get; }

	/// <summary>
	/// Gets the display name of this user (global name if available, otherwise username).
	/// </summary>
	string DisplayName { get; }

	/// <summary>
	/// Gets a REST action to create or get a direct message channel with this user.
	/// </summary>
	/// <returns>A REST action that can be executed to get or create the DM channel.</returns>
	/// <remarks>
	/// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
	/// </remarks>
	IRestAction<IDmChannel> CreateDM();
}
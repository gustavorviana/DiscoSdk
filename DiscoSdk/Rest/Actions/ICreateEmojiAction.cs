using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a REST action for creating a Discord emoji.
/// </summary>
public interface ICreateEmojiAction : IRestAction<IEmoji>
{
	/// <summary>
	/// Sets the name of the emoji.
	/// </summary>
	/// <param name="name">The name of the emoji.</param>
	/// <returns>The current <see cref="ICreateEmojiAction"/> instance.</returns>
	ICreateEmojiAction SetName(string name);

	/// <summary>
	/// Sets the image data for the emoji.
	/// </summary>
	/// <param name="image">The image data.</param>
	/// <returns>The current <see cref="ICreateEmojiAction"/> instance.</returns>
	ICreateEmojiAction SetImage(DiscordImageBuffer image);

	/// <summary>
	/// Sets the roles that can use this emoji.
	/// </summary>
	/// <param name="roleIds">The role IDs that can use this emoji.</param>
	/// <returns>The current <see cref="ICreateEmojiAction"/> instance.</returns>
	ICreateEmojiAction SetRoles(params Snowflake[] roleIds);
}


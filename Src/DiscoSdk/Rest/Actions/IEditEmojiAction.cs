using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a REST action for editing a Discord emoji.
/// </summary>
public interface IEditEmojiAction : IRestAction<IEmoji>
{
	/// <summary>
	/// Sets the name of the emoji.
	/// </summary>
	/// <param name="name">The name of the emoji.</param>
	/// <returns>The current <see cref="IEditEmojiAction"/> instance.</returns>
	IEditEmojiAction SetName(string name);

	/// <summary>
	/// Sets the roles that can use this emoji.
	/// </summary>
	/// <param name="roleIds">The role IDs that can use this emoji.</param>
	/// <returns>The current <see cref="IEditEmojiAction"/> instance.</returns>
	IEditEmojiAction SetRoles(params Snowflake[] roleIds);
}


using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a Discord emoji.
/// </summary>
public interface IEmoji : IMentionable, IDeletable
{
	/// <summary>
	/// Gets the emoji's name.
	/// </summary>
	string? Name { get; }

	/// <summary>
	/// Gets the roles allowed to use this emoji.
	/// </summary>
	string[] Roles { get; }

	/// <summary>
	/// Gets the user that created this emoji.
	/// </summary>
	IUser? User { get; }

	/// <summary>
	/// Gets a value indicating whether this emoji must be wrapped in colons.
	/// </summary>
	bool RequireColons { get; }

	/// <summary>
	/// Gets a value indicating whether this emoji is managed.
	/// </summary>
	bool IsManaged { get; }

	/// <summary>
	/// Gets a value indicating whether this emoji is animated.
	/// </summary>
	bool IsAnimated { get; }

	/// <summary>
	/// Gets a value indicating whether this emoji is available.
	/// </summary>
	bool Available { get; }

	/// <summary>
	/// Gets the guild this emoji belongs to.
	/// </summary>
	IGuild? Guild { get; }

	/// <summary>
	/// Gets a REST action for editing this emoji.
	/// </summary>
	/// <returns>A REST action that can be configured and executed to edit the emoji.</returns>
	/// <remarks>
	/// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
	/// </remarks>
	IEditEmojiAction Edit();
}

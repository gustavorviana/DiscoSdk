using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Messages;

namespace DiscoSdk.Models.Interactions;

/// <summary>
/// Represents resolved data from an interaction, containing users, members, roles, channels, and messages.
/// </summary>
/// <remarks>
/// All Discord IDs in the dictionary keys must be of type <see cref="Snowflake"/> (as strings).
/// </remarks>
public interface IInteractionResolved
{
	/// <summary>
	/// Gets the resolved users, keyed by their Discord ID as a string.
	/// </summary>
	IReadOnlyCollection<IUser> Users { get; }

	/// <summary>
	/// Gets the resolved members, keyed by their Discord ID as a string.
	/// </summary>
	IReadOnlyCollection<IMember> Members { get; }

	/// <summary>
	/// Gets the resolved roles, keyed by their Discord ID as a string.
	/// </summary>
	IReadOnlyCollection<IRole> Roles { get; }

	/// <summary>
	/// Gets the resolved channels, keyed by their Discord ID as a string.
	/// </summary>
	IReadOnlyCollection<IChannel> Channels { get; }

	/// <summary>
	/// Gets the resolved messages, keyed by their Discord ID as a string.
	/// </summary>
	IReadOnlyCollection<IMessage> Messages { get; }
}
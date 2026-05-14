using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Messages;

namespace DiscoSdk.Models.Interactions;

// Public surface of an interaction's `resolved` block. The Attachments lookup is populated by
// Discord when a modal submission includes a FileUpload component (the IDs in the field's
// Values array map into this dictionary).

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

	/// <summary>
	/// Gets the resolved attachments — populated when a modal submission includes a
	/// <c>FileUpload</c> component. Keyed by attachment ID.
	/// </summary>
	IReadOnlyDictionary<string, Attachment> Attachments { get; }
}
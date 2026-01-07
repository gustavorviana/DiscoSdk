using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Models.Messages.Embeds;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a REST action for editing a message in Discord.
/// </summary>
public interface IEditMessageRestAction : IRestAction<IMessage>
{
	/// <summary>
	/// Sets the content of the message.
	/// </summary>
	/// <param name="content">The message content (max 2000 characters).</param>
	/// <returns>The current <see cref="IEditMessageRestAction"/> instance.</returns>
	IEditMessageRestAction SetContent(string? content);

	/// <summary>
	/// Sets the embeds of the message.
	/// </summary>
	/// <param name="embeds">The embeds to set.</param>
	/// <returns>The current <see cref="IEditMessageRestAction"/> instance.</returns>
	IEditMessageRestAction SetEmbeds(params Embed[] embeds);

	/// <summary>
	/// Sets the components (action rows) of the message.
	/// Components should already be ActionRows or will be automatically wrapped.
	/// </summary>
	/// <param name="components">The components (action rows) to set.</param>
	/// <returns>The current <see cref="IEditMessageRestAction"/> instance.</returns>
	IEditMessageRestAction SetComponents(params MessageComponent[] components);

	/// <summary>
	/// Sets the allowed mentions for the message.
	/// </summary>
	/// <param name="parse">The types of mentions to parse (users, roles, everyone).</param>
	/// <param name="users">Specific user IDs to mention.</param>
	/// <param name="roles">Specific role IDs to mention.</param>
	/// <returns>The current <see cref="IEditMessageRestAction"/> instance.</returns>
	IEditMessageRestAction SetAllowedMentions(string[]? parse = null, string[]? users = null, string[]? roles = null);

	/// <summary>
	/// Sets the message flags.
	/// </summary>
	/// <param name="flags">The message flags to set.</param>
	/// <returns>The current <see cref="IEditMessageRestAction"/> instance.</returns>
	IEditMessageRestAction SetFlags(MessageFlags? flags);
}
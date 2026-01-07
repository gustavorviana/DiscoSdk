using DiscoSdk.Models.Builders;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Models.Messages.Embeds;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a REST action for sending a message to Discord.
/// </summary>
public interface ISendMessageRestAction : IRestAction<Message>
{
    /// <summary>
    /// Sets the content of the message.
    /// </summary>
    /// <param name="content">The message content (max 2000 characters).</param>
    /// <returns>The current <see cref="ISendMessageRestAction"/> instance.</returns>
    ISendMessageRestAction SetContent(string? content);

    /// <summary>
    /// Adds an embed to the message.
    /// </summary>
    /// <param name="embeds">The embeds to add.</param>
    /// <returns>The current <see cref="ISendMessageRestAction"/> instance.</returns>
    ISendMessageRestAction AddEmbeds(params Embed[] embeds);

    /// <summary>
    /// Adds an action row containing the specified components.
    /// Components will be automatically wrapped in an ActionRow if they are not already.
    /// </summary>
    /// <param name="items">The components to add to the action row (buttons, select menus, etc.).</param>
    /// <returns>The current <see cref="ISendMessageRestAction"/> instance.</returns>
    ISendMessageRestAction AddActionRow(params MessageComponent[] items);

    /// <summary>
    /// Sets the components (action rows) of the message.
    /// Components should already be ActionRows or will be automatically wrapped.
    /// </summary>
    /// <param name="components">The components (action rows) to set.</param>
    /// <returns>The current <see cref="ISendMessageRestAction"/> instance.</returns>
    ISendMessageRestAction SetComponents(params MessageComponent[] components);

    /// <summary>
    /// Sets whether the message should be sent as TTS (text-to-speech).
    /// </summary>
    /// <param name="tts">True if the message should be TTS, false otherwise.</param>
    /// <returns>The current <see cref="ISendMessageRestAction"/> instance.</returns>
    ISendMessageRestAction SetTts(bool tts);

    /// <summary>
    /// Sets the message reference (reply to a message).
    /// </summary>
    /// <param name="messageId">The ID of the message to reply to.</param>
    /// <param name="channelId">The ID of the channel the message is in.</param>
    /// <param name="guildId">The ID of the guild the message is in (optional).</param>
    /// <param name="failIfNotExists">Whether to fail if the referenced message doesn't exist.</param>
    /// <returns>The current <see cref="ISendMessageRestAction"/> instance.</returns>
    ISendMessageRestAction SetMessageReference(string? messageId, string? channelId = null, string? guildId = null, bool? failIfNotExists = null);

    /// <summary>
    /// Sets the allowed mentions for the message.
    /// </summary>
    /// <param name="parse">The types of mentions to parse (users, roles, everyone).</param>
    /// <param name="users">Specific user IDs to mention.</param>
    /// <param name="roles">Specific role IDs to mention.</param>
    /// <returns>The current <see cref="ISendMessageRestAction"/> instance.</returns>
    ISendMessageRestAction SetAllowedMentions(string[]? parse = null, string[]? users = null, string[]? roles = null);

    /// <summary>
    /// Sets whether the message should be ephemeral (only visible to the user who triggered the interaction).
    /// Note: Ephemeral messages are only supported for interaction responses, not regular channel messages.
    /// </summary>
    /// <param name="ephemeral">True if the message should be ephemeral, false otherwise.</param>
    /// <returns>The current <see cref="ISendMessageRestAction"/> instance.</returns>
    ISendMessageRestAction SetEphemeral(bool ephemeral = true);
}


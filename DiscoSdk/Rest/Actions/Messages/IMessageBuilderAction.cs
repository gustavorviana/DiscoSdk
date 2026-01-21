using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Models.Messages.Embeds;
using DiscoSdk.Models.Messages.Pools;
using DiscoSdk.Models.Requests.Messages;

namespace DiscoSdk.Rest.Actions.Messages;

public interface IMessageBuilderAction<TSelf, TMessage> : IRestAction<TMessage>
{
    /// <summary>
    /// Sets the content of the message.
    /// </summary>
    /// <param name="content">The message content (max 2000 characters).</param>
    /// <returns>The current <see cref="TSelf"/> instance.</returns>
    TSelf SetContent(string? content);

    /// <summary>
    /// Sets the embeds of the message.
    /// </summary>
    /// <param name="embeds">The embeds to set.</param>
    /// <returns>The current <see cref="TSelf"/> instance.</returns>
    TSelf SetEmbeds(params Embed[] embeds);

    /// <summary>
    /// Adds an action row containing the specified components.
    /// Components will be automatically wrapped in an ActionRow if they are not already.
    /// </summary>
    /// <param name="items">The components to add to the action row (buttons, select menus, etc.).</param>
    /// <returns>The current <see cref="ISendMessageRestAction"/> instance.</returns>
    TSelf AddActionRow(params MessageComponent[] items);

    TSelf ClearComponents();

    /// <summary>
    /// Enables or disables embed rendering for the message.
    /// When set to <c>true</c>, all embeds in the message will be suppressed and not rendered by clients.
    /// When set to <c>false</c>, embeds will be restored and rendered normally.
    /// </summary>
    /// <param name="suppress">
    /// <c>true</c> to suppress embeds; <c>false</c> to allow embeds to be rendered.
    /// </param>
    /// <returns>The current <see cref="TSelf"/> instance.</returns>
    TSelf SetSuppressEmbeds(bool suppress = true);

    TSelf SetPoll(Poll? poll);

    TSelf ClearAttachments();

    TSelf SetAllowedMentions(AllowedMentions? allowedMentions);
}
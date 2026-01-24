using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Models.Messages.Embeds;
using DiscoSdk.Models.Messages.Mentions;
using DiscoSdk.Models.Messages.Pools;

namespace DiscoSdk.Rest.Actions.Messages;

/// <summary>
/// Defines a fluent builder contract for creating or editing Discord messages.
/// Implementations accumulate message state and execute a REST action that
/// produces a <typeparamref name="TMessage"/>.
/// </summary>
/// <typeparam name="TSelf">The concrete builder type (CRTP pattern).</typeparam>
/// <typeparam name="TMessage">The resulting message model type.</typeparam>
public interface IMessageBuilderAction<TSelf, TMessage> : IRestAction<TMessage>
{
    /// <summary>
    /// Sets the raw textual content of the message.
    /// </summary>
    /// <param name="content">The message content (maximum 2000 characters).</param>
    /// <returns>The current <see cref="TSelf"/> instance.</returns>
    TSelf SetContent(string? content);

    /// <summary>
    /// Sets the textual content using a <see cref="MessageTextBuilder"/>,
    /// allowing fine-grained control over mentions and formatting.
    /// </summary>
    /// <param name="builder">The text builder used to compose the message.</param>
    /// <returns>The current <see cref="TSelf"/> instance.</returns>
    TSelf SetContent(MessageTextBuilder builder);

    /// <summary>
    /// Replaces all embeds in the message.
    /// </summary>
    /// <param name="embeds">The embeds to set.</param>
    /// <returns>The current <see cref="TSelf"/> instance.</returns>
    TSelf SetEmbeds(params Embed[] embeds);

    /// <summary>
    /// Adds an action row containing the specified components.
    /// Components will be wrapped in an ActionRow when required.
    /// </summary>
    /// <param name="items">The components to add (buttons, selects, etc.).</param>
    /// <returns>The current <see cref="TSelf"/> instance.</returns>
    TSelf AddActionRow(params MessageComponent[] items);

    /// <summary>
    /// Removes all components from the message.
    /// </summary>
    /// <returns>The current <see cref="TSelf"/> instance.</returns>
    TSelf ClearComponents();

    /// <summary>
    /// Enables or disables embed rendering for the message.
    /// When <c>true</c>, all embeds will be suppressed by clients.
    /// When <c>false</c>, embeds will be rendered normally.
    /// </summary>
    /// <param name="suppress">
    /// <c>true</c> to suppress embeds; <c>false</c> to allow rendering.
    /// </param>
    /// <returns>The current <see cref="TSelf"/> instance.</returns>
    TSelf SetSuppressEmbeds(bool suppress = true);

    /// <summary>
    /// Attaches a file to the message.
    /// </summary>
    /// <param name="file">The file to attach.</param>
    /// <returns>The current <see cref="TSelf"/> instance.</returns>
    TSelf AttachFile(MessageFile file);

    /// <summary>
    /// Attaches multiple files to the message.
    /// </summary>
    /// <param name="files">The files to attach.</param>
    /// <returns>The current <see cref="TSelf"/> instance.</returns>
    TSelf AttachFiles(params MessageFile[] files);

    /// <summary>
    /// Removes all file attachments from the message.
    /// </summary>
    /// <returns>The current <see cref="TSelf"/> instance.</returns>
    TSelf ClearAttachments();

    /// <summary>
    /// Sets the <c>allowed_mentions</c> policy for the message using a
    /// <see cref="MentionBuilder"/>.
    ///
    /// This overload is intended for scenarios where mention behavior is composed
    /// semantically (users, roles, everyone, silent vs. ping) and then translated
    /// into Discord’s low-level <c>allowed_mentions</c> payload.
    ///
    /// The provided <paramref name="builder"/> encapsulates:
    /// - Which entities were mentioned (users, roles, everyone)
    /// - Which mentions should actually notify (ping)
    /// - The rules required to safely generate <c>parse</c>, <c>users</c> and <c>roles</c>
    ///   without accidentally pinging silent mentions
    ///
    /// Using this method avoids dealing directly with Discord’s raw arrays and
    /// prevents subtle mistakes such as enabling global parsing when silent mentions
    /// are present.
    /// </summary>
    /// <param name="builder">
    /// A mention builder containing the semantic mention state to be converted into
    /// an <see cref="AllowedMentions"/> payload.
    /// </param>
    /// <returns>The current <see cref="TSelf"/> instance.</returns>
    TSelf SetAllowedMentions(MentionBuilder builder);
}
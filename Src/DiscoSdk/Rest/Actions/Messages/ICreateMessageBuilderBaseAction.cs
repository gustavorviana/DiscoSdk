using DiscoSdk.Models.Messages;

namespace DiscoSdk.Rest.Actions.Messages;

public interface ICreateMessageBuilderBaseAction<TSelf, TMessage> : IMessageBuilderAction<TSelf, TMessage>
{
    /// <summary>
    /// Enables or disables text-to-speech for this message.
    /// </summary>
    /// <param name="tts"><c>true</c> to enable TTS; otherwise <c>false</c>.</param>
    /// <returns>The current <see cref="TSelf"/> instance.</returns>
    TSelf SetTts(bool tts = true);

    /// <summary>
    /// Enables or disables notification suppression for this message (silent message).
    /// </summary>
    /// <param name="suppress">
    /// <c>true</c> to suppress push/desktop notifications; <c>false</c> to allow notifications.
    /// </param>
    /// <returns>The current <see cref="TSelf"/> instance.</returns>
    TSelf SetSuppressNotifications(bool suppress = true);

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
}
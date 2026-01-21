using DiscoSdk.Models.Messages;

namespace DiscoSdk.Rest.Actions.Messages;

/// <summary>
/// Represents a REST action for sending a message to Discord.
/// </summary>
public interface ISendMessageRestAction : ICreateMessageBuilderBaseAction<ISendMessageRestAction, IMessage>, IBotMessageBuilder<ISendMessageRestAction>
{
    /// Sets whether the message should be ephemeral (only visible to the user who triggered the interaction).
    /// Note: Ephemeral messages are only supported for interaction responses, not regular channel messages.
    /// </summary>
    /// <param name="ephemeral">True if the message should be ephemeral, false otherwise.</param>
    /// <returns>The current <see cref="ISendMessageRestAction"/> instance.</returns>
    ISendMessageRestAction SetEphemeral(bool ephemeral = true);
}
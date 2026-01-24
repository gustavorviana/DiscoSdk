namespace DiscoSdk.Models.Enums;

/// <summary>
/// Represents message flags.
/// <para>
/// Reference:
/// https://discord.com/developers/docs/resources/message#message-object-message-flags
/// </para>
/// </summary>
[Flags]
public enum MessageFlags
{
    /// <summary>
    /// No flags are set.
    /// </summary>
    None = 0,

    /// <summary>
    /// This message has been published to subscribed channels (via Channel Following).
    /// </summary>
    Crossposted = 1 << 0,

    /// <summary>
    /// This message originated from a crosspost from another channel.
    /// </summary>
    IsCrosspost = 1 << 1,

    /// <summary>
    /// Embeds in this message have been suppressed.
    /// Can be toggled via message edit.
    /// </summary>
    SuppressEmbeds = 1 << 2,

    /// <summary>
    /// The source message for this crosspost has been deleted.
    /// </summary>
    SourceMessageDeleted = 1 << 3,

    /// <summary>
    /// This message is marked as urgent.
    /// Used for system-generated urgent messages.
    /// </summary>
    Urgent = 1 << 4,

    /// <summary>
    /// This message has an associated thread.
    /// </summary>
    HasThread = 1 << 5,

    /// <summary>
    /// This message is ephemeral and is only visible to the user who triggered it.
    /// Only valid for interaction responses.
    /// </summary>
    Ephemeral = 1 << 6,

    /// <summary>
    /// This message is an interaction response and the bot is still "thinking".
    /// The loading state is shown to the user.
    /// </summary>
    Loading = 1 << 7,

    /// <summary>
    /// Some roles could not be mentioned in this thread message.
    /// </summary>
    FailedToMentionSomeRolesInThread = 1 << 8,

    /// <summary>
    /// This message will not trigger push or desktop notifications.
    /// Can be toggled via message edit.
    /// </summary>
    SuppressNotifications = 1 << 12,

    /// <summary>
    /// This message represents a voice message.
    /// </summary>
    IsVoiceMessage = 1 << 13,

    /// <summary>
    /// This message has a snapshot attached.
    /// </summary>
    HasSnapshot = 1 << 14,

    /// <summary>
    /// This message uses the newer Component V2 system.
    /// </summary>
    IsComponentV2 = 1 << 15
}

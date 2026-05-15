namespace DiscoSdk.Commands;

/// <summary>
/// Selects which context menu variant a <see cref="ContextMenuBuilder"/> produces on
/// <see cref="ContextMenuBuilder.Build(ContextMenuType)"/>. Maps 1:1 to the user-relevant subset
/// of <see cref="DiscoSdk.Models.Enums.ApplicationCommandType"/>; the values that don't make sense
/// here (ChatInput / PrimaryEntryPoint) are intentionally excluded.
/// </summary>
public enum ContextMenuType
{
    /// <summary>
    /// User context menu — appears when right-clicking a member. The interaction receives
    /// the target user as <c>data.target_id</c> at runtime.
    /// </summary>
    User,

    /// <summary>
    /// Message context menu — appears when right-clicking a message. The interaction receives
    /// the target message as <c>data.target_id</c> at runtime.
    /// </summary>
    Message,
}

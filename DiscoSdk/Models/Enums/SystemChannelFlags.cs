namespace DiscoSdk.Models.Enums;

/// <summary>
/// Represents system channel flags.
/// </summary>
[Flags]
public enum SystemChannelFlags
{
    None = 0,
    SuppressJoinNotifications = 1 << 0,
    SuppressPremiumSubscriptions = 1 << 1,
    SuppressGuildReminderNotifications = 1 << 2,
    SuppressJoinNotificationReplies = 1 << 3
}

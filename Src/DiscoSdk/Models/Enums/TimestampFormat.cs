namespace DiscoSdk.Models.Enums;

/// <summary>
/// Represents the display format for Discord timestamp mentions.
/// </summary>
/// <remarks>
/// Discord timestamps use the format <c>&lt;t:UNIX_TIMESTAMP:FORMAT&gt;</c> where FORMAT is one of these values.
/// For more information, see the Discord API documentation:
/// <see href="https://discord.com/developers/docs/reference#message-formatting-timestamp-styles"/>
/// </remarks>
public enum TimestampFormat
{
    /// <summary>
    /// Short time format (e.g., "12:00 AM").
    /// </summary>
    /// <remarks>
    /// Displays time in 12-hour format with AM/PM.
    /// </remarks>
    ShortTime = 't',

    /// <summary>
    /// Long time format (e.g., "12:00:00 AM").
    /// </summary>
    /// <remarks>
    /// Displays time in 12-hour format with seconds and AM/PM.
    /// </remarks>
    LongTime = 'T',

    /// <summary>
    /// Short date format (e.g., "01/01/2023").
    /// </summary>
    /// <remarks>
    /// Displays date in MM/DD/YYYY format.
    /// </remarks>
    ShortDate = 'd',

    /// <summary>
    /// Long date format (e.g., "January 1, 2023").
    /// </summary>
    /// <remarks>
    /// Displays date with full month name.
    /// </remarks>
    LongDate = 'D',

    /// <summary>
    /// Short date and time format (e.g., "January 1, 2023 12:00 AM").
    /// </summary>
    /// <remarks>
    /// Displays both date and time in a compact format.
    /// This is the default format used by Discord.
    /// </remarks>
    ShortDateTime = 'f',

    /// <summary>
    /// Long date and time format (e.g., "Sunday, January 1, 2023 12:00 AM").
    /// </summary>
    /// <remarks>
    /// Displays full date with day of week, month, day, year, and time.
    /// </remarks>
    LongDateTime = 'F',

    /// <summary>
    /// Relative time format (e.g., "in 5 minutes", "2 hours ago").
    /// </summary>
    /// <remarks>
    /// Displays time relative to the current moment. Updates dynamically.
    /// </remarks>
    RelativeTime = 'R'
}

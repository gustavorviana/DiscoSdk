using DiscoSdk.Models.Enums;

namespace DiscoSdk.Models.Activities;

/// <summary>
/// Read-only view of a Discord activity (rich presence entry) attached to a user. The SDK wraps
/// the gateway POCO so consumers cannot mutate cached state through the public surface — use the
/// <see cref="DiscoSdk.Rest.Actions.IActivityBuilder"/> family
/// (<see cref="PlayingActivity"/>, <see cref="StreamingActivity"/>, ...) when the bot needs to
/// publish its own presence instead.
/// </summary>
public interface IActivity
{
    /// <summary>The activity name (game title, custom-status placeholder, Spotify track, ...).</summary>
    string Name { get; }

    /// <summary>The activity type (Playing, Streaming, Listening, Watching, Custom, Competing).</summary>
    ActivityType Type { get; }

    /// <summary>The stream URL when <see cref="Type"/> is <see cref="ActivityType.Streaming"/>, otherwise <c>null</c>.</summary>
    string? Url { get; }

    /// <summary>When the activity was created, or <c>null</c> when Discord did not report it.</summary>
    DateTimeOffset? CreatedAt { get; }

    /// <summary>Optional start/end timestamps that render as activity progress bars on the Discord client.</summary>
    IActivityTimestamps? Timestamps { get; }

    /// <summary>The application id associated with the activity, when present.</summary>
    Snowflake? ApplicationId { get; }

    /// <summary>Optional first detail line shown under the activity (e.g. game level).</summary>
    string? Details { get; }

    /// <summary>Optional second detail line shown under the activity (e.g. party state).</summary>
    string? State { get; }

    /// <summary>Emoji attached to the activity — primarily used for <see cref="ActivityType.Custom"/>.</summary>
    IActivityEmoji? Emoji { get; }

    /// <summary>Optional party information (current/max size and id).</summary>
    IActivityParty? Party { get; }

    /// <summary>Optional Rich Presence assets (large/small image + hover text).</summary>
    IActivityAssets? Assets { get; }

    /// <summary>Optional Rich Presence secrets exposing join / spectate / match handshake tokens.</summary>
    IActivitySecrets? Secrets { get; }

    /// <summary>
    /// Whether the activity is reported as an instanced game session. Defaults to <c>false</c>
    /// when Discord omits the field.
    /// </summary>
    bool Instance { get; }

    /// <summary>Bitfield of <see cref="ActivityFlag"/> values describing join/spectate/embedded properties.</summary>
    ActivityFlag Flags { get; }

    /// <summary>
    /// Button labels (maximum two) shown under the activity for Rich Presence. Empty when no
    /// buttons were declared — never <c>null</c>.
    /// </summary>
    string[] Buttons { get; }
}

namespace DiscoSdk.Models.Enums;

/// <summary>
/// Bitfield of Discord Rich Presence activity properties. These flags are how a Discord client
/// decides whether to render "Join", "Spectate", and similar action buttons next to a user's
/// activity, and whether the activity is an embedded application running inside Discord.
/// </summary>
[Flags]
public enum ActivityFlag
{
    /// <summary>No flags are set.</summary>
    None = 0,

    /// <summary>The activity represents an instanced game session (has an instance id).</summary>
    Instance = 1 << 0,

    /// <summary>The activity exposes a Join action — other users can request to join the session.</summary>
    Join = 1 << 1,

    /// <summary>The activity exposes a Spectate action — other users can watch the session.</summary>
    Spectate = 1 << 2,

    /// <summary>The activity exposes a Join-Request prompt — a user has asked to join the party.</summary>
    JoinRequest = 1 << 3,

    /// <summary>The activity is synced with an external service (e.g. Spotify).</summary>
    Sync = 1 << 4,

    /// <summary>The activity can be played within Discord.</summary>
    Play = 1 << 5,

    /// <summary>Party privacy is limited to the user's friends.</summary>
    PartyPrivacyFriends = 1 << 6,

    /// <summary>Party privacy is limited to the voice channel the user is in.</summary>
    PartyPrivacyVoiceChannel = 1 << 7,

    /// <summary>The activity is an embedded Discord application (Watch Together, Poker Night, etc.).</summary>
    Embedded = 1 << 8
}

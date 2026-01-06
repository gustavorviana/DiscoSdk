namespace DiscoSdk.Hosting.Gateway;

/// <summary>
/// Discord Gateway Opcode definitions (official values).
/// </summary>
internal enum OpCodes
{
    /// <summary>
    /// 0 — Dispatch
    /// Receive.
    /// An event was dispatched.
    /// </summary>
    Dispatch = 0,

    /// <summary>
    /// 1 — Heartbeat
    /// Send / Receive.
    /// Fired periodically by the client to keep the connection alive.
    /// </summary>
    Heartbeat = 1,

    /// <summary>
    /// 2 — Identify
    /// Send.
    /// Starts a new session during the initial handshake.
    /// </summary>
    Identify = 2,

    /// <summary>
    /// 3 — Presence Update
    /// Send.
    /// Update the client's presence.
    /// </summary>
    PresenceUpdate = 3,

    /// <summary>
    /// 4 — Voice State Update
    /// Send.
    /// Used to join, leave, or move between voice channels.
    /// </summary>
    VoiceStateUpdate = 4,

    /// <summary>
    /// 6 — Resume
    /// Send.
    /// Resume a previous session that was disconnected.
    /// </summary>
    Resume = 6,

    /// <summary>
    /// 7 — Reconnect
    /// Receive.
    /// You should attempt to reconnect and resume immediately.
    /// </summary>
    Reconnect = 7,

    /// <summary>
    /// 8 — Request Guild Members
    /// Send.
    /// Request information about offline guild members in a large guild.
    /// </summary>
    RequestGuildMembers = 8,

    /// <summary>
    /// 9 — Invalid Session
    /// Receive.
    /// The session has been invalidated. You should reconnect and identify/resume accordingly.
    /// </summary>
    InvalidSession = 9,

    /// <summary>
    /// 10 — Hello
    /// Receive.
    /// Sent immediately after connecting, contains the heartbeat_interval to use.
    /// </summary>
    Hello = 10,

    /// <summary>
    /// 11 — Heartbeat ACK
    /// Receive.
    /// Sent in response to receiving a heartbeat to acknowledge that it has been received.
    /// </summary>
    HeartbeatAck = 11,

    /// <summary>
    /// 31 — Request Soundboard Sounds
    /// Send.
    /// Request information about soundboard sounds in a set of guilds.
    /// </summary>
    RequestSoundboardSounds = 31
}


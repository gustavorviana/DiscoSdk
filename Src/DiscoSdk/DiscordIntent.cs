namespace DiscoSdk;

[Flags]
public enum DiscordIntent
{
    /// <summary>
    /// This intent includes no events.
    /// </summary>
    None = 0,

    /// <summary>
    /// Events related to guild lifecycle (create, update, delete, etc).
    /// </summary>
    Guilds = 1 << 0,

    /// <summary>
    /// Events related to guild members (join, leave, update).
    /// Privileged intent.
    /// </summary>
    GuildMembers = 1 << 1,

    /// <summary>
    /// Events related to guild moderation (bans, unbans, timeouts).
    /// </summary>
    GuildModeration = 1 << 2,

    /// <summary>
    /// Events related to guild expressions (emojis and stickers).
    /// </summary>
    GuildExpressions = 1 << 3,

    /// <summary>
    /// Events related to guild integrations.
    /// </summary>
    GuildIntegrations = 1 << 4,

    /// <summary>
    /// Events related to guild webhooks.
    /// </summary>
    GuildWebhooks = 1 << 5,

    /// <summary>
    /// Events related to guild invites.
    /// </summary>
    GuildInvites = 1 << 6,

    /// <summary>
    /// Events related to voice state updates.
    /// </summary>
    GuildVoiceStates = 1 << 7,

    /// <summary>
    /// Events related to member presence updates.
    /// Privileged intent.
    /// </summary>
    GuildPresences = 1 << 8,

    /// <summary>
    /// Events related to messages sent in guilds.
    /// </summary>
    GuildMessages = 1 << 9,

    /// <summary>
    /// Events related to reactions on guild messages.
    /// </summary>
    GuildMessageReactions = 1 << 10,

    /// <summary>
    /// Events related to typing indicators in guilds.
    /// </summary>
    GuildMessageTyping = 1 << 11,

    /// <summary>
    /// Events related to direct messages.
    /// </summary>
    DirectMessages = 1 << 12,

    /// <summary>
    /// Events related to reactions on direct messages.
    /// </summary>
    DirectMessageReactions = 1 << 13,

    /// <summary>
    /// Events related to typing indicators in direct messages.
    /// </summary>
    DirectMessageTyping = 1 << 14,

    /// <summary>
    /// Provides message content data.
    /// Privileged intent.
    /// </summary>
    MessageContent = 1 << 15,

    /// <summary>
    /// Events related to scheduled guild events.
    /// </summary>
    GuildScheduledEvents = 1 << 16,

    /// <summary>
    /// Events related to auto moderation rule configuration.
    /// </summary>
    AutoModerationConfiguration = 1 << 20,

    /// <summary>
    /// Events related to auto moderation rule execution.
    /// </summary>
    AutoModerationExecution = 1 << 21,

    /// <summary>
    /// Events related to polls in guild messages.
    /// </summary>
    GuildMessagePolls = 1 << 24,

    /// <summary>
    /// Events related to polls in direct messages.
    /// </summary>
    DirectMessagePolls = 1 << 25,

    /// <summary>
    /// Includes all non-privileged intents.
    /// Excludes <see cref="GuildMembers"/>, <see cref="GuildPresences"/> and <see cref="MessageContent"/>.
    /// </summary>
    AllUnprivileged =
        Guilds |
        GuildModeration |
        GuildExpressions |
        GuildIntegrations |
        GuildWebhooks |
        GuildInvites |
        GuildVoiceStates |
        GuildMessages |
        GuildMessageReactions |
        GuildMessageTyping |
        DirectMessages |
        DirectMessageReactions |
        DirectMessageTyping |
        GuildScheduledEvents |
        AutoModerationConfiguration |
        AutoModerationExecution |
        GuildMessagePolls |
        DirectMessagePolls,

    /// <summary>
    /// Includes all intents, including privileged ones.
    /// </summary>
    All =
        AllUnprivileged |
        GuildMembers |
        GuildPresences |
        MessageContent
}
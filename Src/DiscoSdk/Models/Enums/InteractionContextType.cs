namespace DiscoSdk.Models.Enums;

/// <summary>
/// Indicates where an interaction (command) can be used.
/// </summary>
public enum InteractionContextType
{
    /// <summary>Interaction can be used inside a server.</summary>
    Guild = 0,

    /// <summary>Interaction can be used in a DM with the bot.</summary>
    BotDm = 1,

    /// <summary>Interaction can be used in group DMs and DMs not involving the bot.</summary>
    PrivateChannel = 2
}

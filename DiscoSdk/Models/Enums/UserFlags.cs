namespace DiscoSdk.Models.Enums;

/// <summary>
/// Represents user flags.
/// </summary>
[Flags]
public enum UserFlags
{
    None = 0,

    /// <summary>Discord employee.</summary>
    Staff = 1 << 0,

    /// <summary>Partnered server owner.</summary>
    Partner = 1 << 1,

    /// <summary>HypeSquad events member.</summary>
    HypeSquad = 1 << 2,

    /// <summary>Bug hunter level 1.</summary>
    BugHunterLevel1 = 1 << 3,

    /// <summary>House Bravery.</summary>
    HypeSquadOnlineHouse1 = 1 << 6,

    /// <summary>House Brilliance.</summary>
    HypeSquadOnlineHouse2 = 1 << 7,

    /// <summary>House Balance.</summary>
    HypeSquadOnlineHouse3 = 1 << 8,

    /// <summary>Early Nitro supporter.</summary>
    PremiumEarlySupporter = 1 << 9,

    /// <summary>User is a team pseudo-user.</summary>
    TeamPseudoUser = 1 << 10,

    /// <summary>Bug hunter level 2.</summary>
    BugHunterLevel2 = 1 << 14,

    /// <summary>Verified bot.</summary>
    VerifiedBot = 1 << 16,

    /// <summary>Early verified bot developer.</summary>
    VerifiedDeveloper = 1 << 17,

    /// <summary>Discord certified moderator.</summary>
    CertifiedModerator = 1 << 18,

    /// <summary>Bot uses HTTP interactions.</summary>
    BotHttpInteractions = 1 << 19,

    /// <summary>Active developer badge.</summary>
    ActiveDeveloper = 1 << 22,

    Quarantined = 1 << 44
}

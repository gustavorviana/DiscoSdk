namespace DiscoSdk.Models.Enums;

/// <summary>
/// Represents guild member flags.
/// </summary>
[Flags]
public enum GuildMemberFlags
{
    None = 0,
    DidRejoin = 1 << 0,
    CompletedOnboarding = 1 << 1,
    BypassesVerification = 1 << 2,
    StartedOnboarding = 1 << 3
}

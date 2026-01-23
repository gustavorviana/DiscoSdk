namespace DiscoSdk.Models.Enums;

/// <summary>
/// Represents flags for a guild member.
/// </summary>
[Flags]
public enum MemberFlag
{
	/// <summary>
	/// No flags.
	/// </summary>
	None = 0,

	/// <summary>
	/// Member did rejoin.
	/// </summary>
	DidRejoin = 1 << 0,

	/// <summary>
	/// Member completed onboarding.
	/// </summary>
	CompletedOnboarding = 1 << 1,

	/// <summary>
	/// Member bypasses verification.
	/// </summary>
	BypassesVerification = 1 << 2,

	/// <summary>
	/// Member started onboarding.
	/// </summary>
	StartedOnboarding = 1 << 3
}


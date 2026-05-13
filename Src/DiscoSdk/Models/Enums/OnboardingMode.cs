namespace DiscoSdk.Models.Enums;

/// <summary>
/// Defines the criteria used to satisfy a guild's onboarding flow.
/// </summary>
public enum OnboardingMode
{
	/// <summary>Counts only default channels towards the onboarding requirements.</summary>
	OnboardingDefault = 0,

	/// <summary>Counts default channels and questions towards the onboarding requirements.</summary>
	OnboardingAdvanced = 1
}

using DiscoSdk.Models;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// A REST action that updates a guild's onboarding configuration. Every field is optional — only the
/// ones that are set are sent.
/// </summary>
public interface IEditGuildOnboardingAction : IRestAction<IGuildOnboarding>
{
	/// <summary>Sets the prompts shown during onboarding.</summary>
	IEditGuildOnboardingAction SetPrompts(params OnboardingPrompt[] prompts);

	/// <summary>Sets the channel IDs that members get opted into automatically.</summary>
	IEditGuildOnboardingAction SetDefaultChannelIds(params Snowflake[] channelIds);

	/// <summary>Sets whether onboarding is enabled.</summary>
	IEditGuildOnboardingAction SetEnabled(bool enabled);

	/// <summary>Sets the onboarding mode.</summary>
	IEditGuildOnboardingAction SetMode(OnboardingMode mode);
}

using DiscoSdk.Models;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// A REST action that updates a guild's onboarding configuration. Every field is optional — only
/// the ones that are set are sent. Discord enforces server-side rules: when <c>Enabled</c> is true
/// and the guild lacks the <c>COMMUNITY</c> feature, <c>DefaultChannelIds</c> must contain at
/// least seven channels; <c>Prompts</c> is capped at 50. The endpoint records changes in the
/// audit log when a reason is attached via <see cref="IRestActionWithReason{TSelf}.WithReason"/>.
/// </summary>
public interface IEditGuildOnboardingAction
	: IRestAction<IGuildOnboarding>, IRestActionWithReason<IEditGuildOnboardingAction>
{
	/// <summary>Sets the prompts shown during onboarding. Max 50.</summary>
	IEditGuildOnboardingAction SetPrompts(params OnboardingPrompt[] prompts);

	/// <summary>Sets the prompts shown during onboarding, built fluently. Max 50.</summary>
	IEditGuildOnboardingAction SetPrompts(params OnboardingPromptBuilder[] prompts);

	/// <summary>
	/// Appends a single prompt configured inline. The callback receives a fresh
	/// <see cref="OnboardingPromptBuilder"/>; <c>Build</c> is called automatically. Repeated
	/// <c>AddPrompt</c> calls accumulate. Calling <see cref="SetPrompts(OnboardingPrompt[])"/>
	/// or <see cref="SetPrompts(OnboardingPromptBuilder[])"/> after <c>AddPrompt</c> resets
	/// the accumulated set.
	/// </summary>
	IEditGuildOnboardingAction AddPrompt(Action<OnboardingPromptBuilder> configure);

	/// <summary>Sets the channel IDs that members get opted into automatically.</summary>
	IEditGuildOnboardingAction SetDefaultChannelIds(params Snowflake[] channelIds);

	/// <summary>Sets whether onboarding is enabled.</summary>
	IEditGuildOnboardingAction SetEnabled(bool enabled);

	/// <summary>Sets the onboarding mode.</summary>
	IEditGuildOnboardingAction SetMode(OnboardingMode mode);
}

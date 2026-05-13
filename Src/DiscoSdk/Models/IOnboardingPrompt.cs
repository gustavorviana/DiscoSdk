using DiscoSdk.Models.Enums;

namespace DiscoSdk.Models;

/// <summary>
/// Read-only view of a question presented to new members during a guild's onboarding flow.
/// </summary>
public interface IOnboardingPrompt
{
	/// <summary>The ID of the prompt.</summary>
	Snowflake Id { get; }

	/// <summary>The type of prompt.</summary>
	OnboardingPromptType Type { get; }

	/// <summary>The options available within the prompt.</summary>
	IReadOnlyList<IOnboardingPromptOption> Options { get; }

	/// <summary>The title of the prompt.</summary>
	string Title { get; }

	/// <summary>Whether members can only select one option for the prompt.</summary>
	bool SingleSelect { get; }

	/// <summary>Whether the prompt is required before a member completes the onboarding flow.</summary>
	bool Required { get; }

	/// <summary>Whether the prompt is present in the onboarding flow; if <c>false</c>, it only appears in Channels &amp; Roles.</summary>
	bool InOnboarding { get; }
}

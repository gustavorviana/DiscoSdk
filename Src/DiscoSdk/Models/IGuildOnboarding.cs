using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models;

/// <summary>
/// A guild's onboarding configuration, with the operations that can be performed on it.
/// </summary>
public interface IGuildOnboarding
{
	/// <summary>The ID of the guild this onboarding is part of.</summary>
	Snowflake GuildId { get; }

	/// <summary>The prompts shown during onboarding and in Channels &amp; Roles.</summary>
	IReadOnlyList<IOnboardingPrompt> Prompts { get; }

	/// <summary>The channel IDs that members get opted into automatically.</summary>
	IReadOnlyList<Snowflake> DefaultChannelIds { get; }

	/// <summary>Whether onboarding is enabled in the guild.</summary>
	bool Enabled { get; }

	/// <summary>The current onboarding mode.</summary>
	OnboardingMode Mode { get; }

	/// <summary>Creates a REST action that updates this onboarding configuration.</summary>
	IEditGuildOnboardingAction Modify();
}

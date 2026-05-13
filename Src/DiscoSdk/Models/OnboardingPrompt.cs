using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// A question presented to new members during a guild's onboarding flow.
/// </summary>
public class OnboardingPrompt
{
	/// <summary>The ID of the prompt.</summary>
	[JsonPropertyName("id")]
	public Snowflake Id { get; set; } = default!;

	/// <summary>The type of prompt.</summary>
	[JsonPropertyName("type")]
	public OnboardingPromptType Type { get; set; }

	/// <summary>The options available within the prompt.</summary>
	[JsonPropertyName("options")]
	public OnboardingPromptOption[] Options { get; set; } = [];

	/// <summary>The title of the prompt.</summary>
	[JsonPropertyName("title")]
	public string Title { get; set; } = default!;

	/// <summary>Whether members can only select one option for the prompt.</summary>
	[JsonPropertyName("single_select")]
	public bool SingleSelect { get; set; }

	/// <summary>Whether the prompt is required before a member completes the onboarding flow.</summary>
	[JsonPropertyName("required")]
	public bool Required { get; set; }

	/// <summary>Whether the prompt is present in the onboarding flow; if <c>false</c>, it only appears in Channels &amp; Roles.</summary>
	[JsonPropertyName("in_onboarding")]
	public bool InOnboarding { get; set; }
}

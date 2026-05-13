using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents the onboarding flow configured for a guild.
/// </summary>
public class GuildOnboarding
{
	/// <summary>The ID of the guild this onboarding is part of.</summary>
	[JsonPropertyName("guild_id")]
	public Snowflake GuildId { get; set; } = default!;

	/// <summary>The prompts shown during onboarding and in Channels &amp; Roles.</summary>
	[JsonPropertyName("prompts")]
	public OnboardingPrompt[] Prompts { get; set; } = [];

	/// <summary>The channel IDs that members get opted into automatically.</summary>
	[JsonPropertyName("default_channel_ids")]
	public Snowflake[] DefaultChannelIds { get; set; } = [];

	/// <summary>Whether onboarding is enabled in the guild.</summary>
	[JsonPropertyName("enabled")]
	public bool Enabled { get; set; }

	/// <summary>The current onboarding mode (which criteria count towards the requirements).</summary>
	[JsonPropertyName("mode")]
	public OnboardingMode Mode { get; set; }
}

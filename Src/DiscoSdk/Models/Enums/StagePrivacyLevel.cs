namespace DiscoSdk.Models.Enums;

/// <summary>
/// Privacy level of a Discord Stage Instance. Mirrors the integer values in Discord's
/// <c>STAGE_INSTANCE</c> privacy_level field.
/// </summary>
public enum StagePrivacyLevel
{
	/// <summary>The stage instance is visible publicly. <i>(Deprecated by Discord.)</i></summary>
	Public = 1,

	/// <summary>The stage instance is visible only to guild members.</summary>
	GuildOnly = 2,
}

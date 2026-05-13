using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models;

/// <summary>
/// Public read/action surface for a Discord Stage Instance — the "live stage" attached to a
/// stage channel.
/// </summary>
public interface IStageInstance
{
	/// <summary>The stage instance id.</summary>
	Snowflake Id { get; }

	/// <summary>The guild this stage is in.</summary>
	Snowflake GuildId { get; }

	/// <summary>The stage channel this instance is bound to.</summary>
	Snowflake ChannelId { get; }

	/// <summary>The topic shown to listeners (1-120 chars).</summary>
	string Topic { get; }

	/// <summary>Who can join the stage.</summary>
	StagePrivacyLevel PrivacyLevel { get; }

	/// <summary>If this stage is linked to a scheduled event, its id.</summary>
	Snowflake? GuildScheduledEventId { get; }

	/// <summary>
	/// Modifies the stage instance's topic and/or privacy level. Pass <c>null</c> for a field
	/// to leave it unchanged.
	/// </summary>
	IRestAction<IStageInstance> Modify(string? topic = null, StagePrivacyLevel? privacyLevel = null);

	/// <summary>Deletes the stage instance, ending the live stage.</summary>
	IRestAction Delete();
}

using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models;

/// <summary>
/// Public read/action surface for a Discord Stage Instance — the "live stage" attached to a
/// stage channel.
/// </summary>
public interface IStageInstance : IReasonedDeletable
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
	/// Creates a builder for modifying the stage instance's topic and/or privacy level. Only
	/// fields touched via the builder are sent on the wire. Chain
	/// <see cref="IRestActionWithReason{TSelf}.WithReason"/> to record the change in the audit log.
	/// </summary>
	IModifyStageInstanceAction Modify();
}

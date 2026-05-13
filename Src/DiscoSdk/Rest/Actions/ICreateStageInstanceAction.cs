using DiscoSdk.Models;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Fluent builder for <c>POST /stage-instances</c>. Required <c>channel_id</c> and <c>topic</c>
/// are supplied via the factory method; everything else flows through this surface.
/// </summary>
public interface ICreateStageInstanceAction : IRestAction<IStageInstance>
{
	/// <summary>Sets the privacy level. Default <see cref="StagePrivacyLevel.GuildOnly"/> when not called.</summary>
	ICreateStageInstanceAction SetPrivacyLevel(StagePrivacyLevel privacyLevel);

	/// <summary>If true, pings <c>@everyone</c> on start. Requires the <c>MENTION_EVERYONE</c> permission.</summary>
	ICreateStageInstanceAction SetSendStartNotification(bool sendStartNotification);

	/// <summary>Links the stage instance to an existing guild scheduled event.</summary>
	ICreateStageInstanceAction SetGuildScheduledEvent(Snowflake guildScheduledEventId);
}

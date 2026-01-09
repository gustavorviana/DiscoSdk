namespace DiscoSdk.Models.Enums;

/// <summary>
/// Extension methods for <see cref="MessageType"/>.
/// </summary>
public static class MessageTypeExtensions
{
	/// <summary>
	/// Determines whether a message of this type can be deleted.
	/// </summary>
	/// <param name="type">The message type.</param>
	/// <returns>True if the message can be deleted, false otherwise.</returns>
	public static bool CanDelete(this MessageType type)
	{
		// Most message types can be deleted, except system messages
		return type != MessageType.ChannelPinnedMessage &&
		       type != MessageType.GuildMemberJoin &&
		       type != MessageType.UserPremiumGuildSubscription &&
		       type != MessageType.UserPremiumGuildSubscriptionTier1 &&
		       type != MessageType.UserPremiumGuildSubscriptionTier2 &&
		       type != MessageType.UserPremiumGuildSubscriptionTier3 &&
		       type != MessageType.ChannelFollowAdd &&
		       type != MessageType.GuildDiscoveryDisqualified &&
		       type != MessageType.GuildDiscoveryRequalified &&
		       type != MessageType.GuildDiscoveryGracePeriodInitialWarning &&
		       type != MessageType.GuildDiscoveryGracePeriodFinalWarning &&
		       type != MessageType.ThreadCreated &&
		       type != MessageType.GuildInviteReminder &&
		       type != MessageType.AutoModerationAction &&
		       type != MessageType.RoleSubscriptionPurchase &&
		       type != MessageType.InteractionPremiumUpsell &&
		       type != MessageType.StageStart &&
		       type != MessageType.StageEnd &&
		       type != MessageType.StageSpeaker &&
		       type != MessageType.StageTopic &&
		       type != MessageType.GuildApplicationPremiumSubscription &&
		       type != MessageType.GuildIncidentAlertModeEnabled &&
		       type != MessageType.GuildIncidentAlertModeDisabled &&
		       type != MessageType.GuildIncidentReportRaid &&
		       type != MessageType.GuildIncidentReportFalseAlarm;
	}
}
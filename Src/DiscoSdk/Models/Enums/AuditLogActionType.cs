namespace DiscoSdk.Models.Enums;

/// <summary>
/// Represents the type of action that occurred in an audit log entry.
/// </summary>
public enum AuditLogActionType
{
	/// <summary>
	/// Guild update.
	/// </summary>
	GuildUpdate = 1,

	/// <summary>
	/// Channel create.
	/// </summary>
	ChannelCreate = 10,

	/// <summary>
	/// Channel update.
	/// </summary>
	ChannelUpdate = 11,

	/// <summary>
	/// Channel delete.
	/// </summary>
	ChannelDelete = 12,

	/// <summary>
	/// Channel overwrite create.
	/// </summary>
	ChannelOverwriteCreate = 13,

	/// <summary>
	/// Channel overwrite update.
	/// </summary>
	ChannelOverwriteUpdate = 14,

	/// <summary>
	/// Channel overwrite delete.
	/// </summary>
	ChannelOverwriteDelete = 15,

	/// <summary>
	/// Member kick.
	/// </summary>
	MemberKick = 20,

	/// <summary>
	/// Member prune.
	/// </summary>
	MemberPrune = 21,

	/// <summary>
	/// Member ban add.
	/// </summary>
	MemberBanAdd = 22,

	/// <summary>
	/// Member ban remove.
	/// </summary>
	MemberBanRemove = 23,

	/// <summary>
	/// Member update.
	/// </summary>
	MemberUpdate = 24,

	/// <summary>
	/// Member role update.
	/// </summary>
	MemberRoleUpdate = 25,

	/// <summary>
	/// Member move.
	/// </summary>
	MemberMove = 26,

	/// <summary>
	/// Member disconnect.
	/// </summary>
	MemberDisconnect = 27,

	/// <summary>
	/// Bot add.
	/// </summary>
	BotAdd = 28,

	/// <summary>
	/// Role create.
	/// </summary>
	RoleCreate = 30,

	/// <summary>
	/// Role update.
	/// </summary>
	RoleUpdate = 31,

	/// <summary>
	/// Role delete.
	/// </summary>
	RoleDelete = 32,

	/// <summary>
	/// Invite create.
	/// </summary>
	InviteCreate = 40,

	/// <summary>
	/// Invite update.
	/// </summary>
	InviteUpdate = 41,

	/// <summary>
	/// Invite delete.
	/// </summary>
	InviteDelete = 42,

	/// <summary>
	/// Webhook create.
	/// </summary>
	WebhookCreate = 50,

	/// <summary>
	/// Webhook update.
	/// </summary>
	WebhookUpdate = 51,

	/// <summary>
	/// Webhook delete.
	/// </summary>
	WebhookDelete = 52,

	/// <summary>
	/// Emoji create.
	/// </summary>
	EmojiCreate = 60,

	/// <summary>
	/// Emoji update.
	/// </summary>
	EmojiUpdate = 61,

	/// <summary>
	/// Emoji delete.
	/// </summary>
	EmojiDelete = 62,

	/// <summary>
	/// Message delete.
	/// </summary>
	MessageDelete = 72,

	/// <summary>
	/// Message bulk delete.
	/// </summary>
	MessageBulkDelete = 73,

	/// <summary>
	/// Message pin.
	/// </summary>
	MessagePin = 74,

	/// <summary>
	/// Message unpin.
	/// </summary>
	MessageUnpin = 75,

	/// <summary>
	/// Integration create.
	/// </summary>
	IntegrationCreate = 80,

	/// <summary>
	/// Integration update.
	/// </summary>
	IntegrationUpdate = 81,

	/// <summary>
	/// Integration delete.
	/// </summary>
	IntegrationDelete = 82,

	/// <summary>
	/// Stage instance create.
	/// </summary>
	StageInstanceCreate = 83,

	/// <summary>
	/// Stage instance update.
	/// </summary>
	StageInstanceUpdate = 84,

	/// <summary>
	/// Stage instance delete.
	/// </summary>
	StageInstanceDelete = 85,

	/// <summary>
	/// Sticker create.
	/// </summary>
	StickerCreate = 90,

	/// <summary>
	/// Sticker update.
	/// </summary>
	StickerUpdate = 91,

	/// <summary>
	/// Sticker delete.
	/// </summary>
	StickerDelete = 92,

	/// <summary>
	/// Scheduled event create.
	/// </summary>
	ScheduledEventCreate = 100,

	/// <summary>
	/// Scheduled event update.
	/// </summary>
	ScheduledEventUpdate = 101,

	/// <summary>
	/// Scheduled event delete.
	/// </summary>
	ScheduledEventDelete = 102,

	/// <summary>
	/// Thread create.
	/// </summary>
	ThreadCreate = 110,

	/// <summary>
	/// Thread update.
	/// </summary>
	ThreadUpdate = 111,

	/// <summary>
	/// Thread delete.
	/// </summary>
	ThreadDelete = 112,

	/// <summary>
	/// Application command permission update.
	/// </summary>
	ApplicationCommandPermissionUpdate = 121,

	/// <summary>
	/// Auto moderation rule create.
	/// </summary>
	AutoModerationRuleCreate = 140,

	/// <summary>
	/// Auto moderation rule update.
	/// </summary>
	AutoModerationRuleUpdate = 141,

	/// <summary>
	/// Auto moderation rule delete.
	/// </summary>
	AutoModerationRuleDelete = 142,

	/// <summary>
	/// Auto moderation action execution.
	/// </summary>
	AutoModerationActionExecution = 143,

	/// <summary>
	/// Creator monetization request created.
	/// </summary>
	CreatorMonetizationRequestCreated = 150,

	/// <summary>
	/// Creator monetization terms accepted.
	/// </summary>
	CreatorMonetizationTermsAccepted = 151
}


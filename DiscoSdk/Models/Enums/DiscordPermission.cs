namespace DiscoSdk.Models.Enums;

/// <summary>
/// Represents Discord permission flags.
/// </summary>
[Flags]
public enum DiscordPermission : ulong
{
	/// <summary>
	/// No permissions.
	/// </summary>
	None = 0,

	/// <summary>
	/// Allows creation of instant invites.
	/// </summary>
	CreateInstantInvite = 1UL << 0,

	/// <summary>
	/// Allows kicking members.
	/// </summary>
	KickMembers = 1UL << 1,

	/// <summary>
	/// Allows banning members.
	/// </summary>
	BanMembers = 1UL << 2,

	/// <summary>
	/// Allows all permissions and bypasses channel permission overwrites.
	/// </summary>
	Administrator = 1UL << 3,

	/// <summary>
	/// Allows management and editing of channels.
	/// </summary>
	ManageChannels = 1UL << 4,

	/// <summary>
	/// Allows management and editing of the guild.
	/// </summary>
	ManageGuild = 1UL << 5,

	/// <summary>
	/// Allows for the addition of reactions to messages.
	/// </summary>
	AddReactions = 1UL << 6,

	/// <summary>
	/// Allows for viewing of audit logs.
	/// </summary>
	ViewAuditLog = 1UL << 7,

	/// <summary>
	/// Allows for using priority speaker in a voice channel.
	/// </summary>
	PrioritySpeaker = 1UL << 8,

	/// <summary>
	/// Allows the user to go live.
	/// </summary>
	Stream = 1UL << 9,

	/// <summary>
	/// Allows guild members to view a channel, which includes reading messages in text channels and joining voice channels.
	/// </summary>
	ViewChannel = 1UL << 10,

	/// <summary>
	/// Allows for sending messages in a channel and creating threads in a forum (does not allow sending messages in threads).
	/// </summary>
	SendMessages = 1UL << 11,

	/// <summary>
	/// Allows for sending of /tts messages.
	/// </summary>
	SendTtsMessages = 1UL << 12,

	/// <summary>
	/// Allows for deletion of other users messages.
	/// </summary>
	ManageMessages = 1UL << 13,

	/// <summary>
	/// Links sent by users with this permission will be auto-embedded.
	/// </summary>
	EmbedLinks = 1UL << 14,

	/// <summary>
	/// Allows for uploading images and files.
	/// </summary>
	AttachFiles = 1UL << 15,

	/// <summary>
	/// Allows for reading of message history.
	/// </summary>
	ReadMessageHistory = 1UL << 16,

	/// <summary>
	/// Allows for using the @everyone and @here mentions.
	/// </summary>
	MentionEveryone = 1UL << 17,

	/// <summary>
	/// Allows the usage of custom emojis from other servers.
	/// </summary>
	UseExternalEmojis = 1UL << 18,

	/// <summary>
	/// Allows for viewing guild insights.
	/// </summary>
	ViewGuildInsights = 1UL << 19,

	/// <summary>
	/// Allows for joining of a voice channel.
	/// </summary>
	Connect = 1UL << 20,

	/// <summary>
	/// Allows for speaking in a voice channel.
	/// </summary>
	Speak = 1UL << 21,

	/// <summary>
	/// Allows for muting members in a voice channel.
	/// </summary>
	MuteMembers = 1UL << 22,

	/// <summary>
	/// Allows for deafening of members in a voice channel.
	/// </summary>
	DeafenMembers = 1UL << 23,

	/// <summary>
	/// Allows for moving of members between voice channels.
	/// </summary>
	MoveMembers = 1UL << 24,

	/// <summary>
	/// Allows for using voice-activity-detection in a voice channel.
	/// </summary>
	UseVad = 1UL << 25,

	/// <summary>
	/// Allows for modification of own nickname.
	/// </summary>
	ChangeNickname = 1UL << 26,

	/// <summary>
	/// Allows for modification of other users nicknames.
	/// </summary>
	ManageNicknames = 1UL << 27,

	/// <summary>
	/// Allows management and editing of roles.
	/// </summary>
	ManageRoles = 1UL << 28,

	/// <summary>
	/// Allows management and editing of webhooks.
	/// </summary>
	ManageWebhooks = 1UL << 29,

	/// <summary>
	/// Allows management and editing of emojis, stickers, and soundboard sounds.
	/// </summary>
	ManageEmojisAndStickers = 1UL << 30,

	/// <summary>
	/// Allows members to use application commands, including slash commands and context menu commands.
	/// </summary>
	UseApplicationCommands = 1UL << 31,

	/// <summary>
	/// Allows for requesting to speak in stage channels.
	/// </summary>
	RequestToSpeak = 1UL << 32,

	/// <summary>
	/// Allows for creating, editing, and deleting scheduled events.
	/// </summary>
	ManageEvents = 1UL << 33,

	/// <summary>
	/// Allows for deleting and archiving threads, and viewing all private threads.
	/// </summary>
	ManageThreads = 1UL << 34,

	/// <summary>
	/// Allows for creating public and announcement threads.
	/// </summary>
	CreatePublicThreads = 1UL << 35,

	/// <summary>
	/// Allows for creating private threads.
	/// </summary>
	CreatePrivateThreads = 1UL << 36,

	/// <summary>
	/// Allows the usage of custom stickers from other servers.
	/// </summary>
	UseExternalStickers = 1UL << 37,

	/// <summary>
	/// Allows for sending messages in threads.
	/// </summary>
	SendMessagesInThreads = 1UL << 38,

	/// <summary>
	/// Allows for using Activities (applications with the EMBEDDED flag) in a voice channel.
	/// </summary>
	UseEmbeddedActivities = 1UL << 39,

	/// <summary>
	/// Allows for timing out users to prevent them from sending or reacting to messages in chat and threads, and from speaking in voice and stage channels.
	/// </summary>
	ModerateMembers = 1UL << 40,

	/// <summary>
	/// Allows for viewing role subscription insights.
	/// </summary>
	ViewCreatorMonetizationAnalytics = 1UL << 41,

	/// <summary>
	/// Allows for using soundboard in a voice channel.
	/// </summary>
	UseSoundboard = 1UL << 42,

	/// <summary>
	/// Allows for creating emojis, stickers, and soundboard sounds.
	/// </summary>
	CreateGuildExpressions = 1UL << 43,

	/// <summary>
	/// Allows for creating scheduled events.
	/// </summary>
	CreateEvents = 1UL << 44,

	/// <summary>
	/// Allows for using external sounds in a voice channel.
	/// </summary>
	UseExternalSounds = 1UL << 45,

	/// <summary>
	/// Allows for sending voice messages.
	/// </summary>
	SendVoiceMessages = 1UL << 46
}


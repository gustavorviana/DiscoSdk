using DiscoSdk.Hosting.Gateway;

namespace DiscoSdk.Hosting.Tests.Gateway.Common;

/// <summary>
/// Builds raw JSON dispatch frames (op=0, t=event-name) and parses them into
/// <see cref="ReceivedGatewayMessage"/>, ready to feed <see cref="DiscoSdk.Hosting.Gateway.Events.DiscordEventDispatcher.ProcessEventAsync"/>.
/// </summary>
internal static class DispatchFrames
{
	public static ReceivedGatewayMessage Dispatch(string eventType, string dataJson) =>
		ReceivedGatewayMessage.Parse($"{{\"op\":0,\"t\":\"{eventType}\",\"s\":1,\"d\":{dataJson}}}");

	public static ReceivedGatewayMessage GuildCreate(ulong id = 100, string name = "TestGuild") =>
		Dispatch("GUILD_CREATE",
			$"{{\"id\":\"{id}\",\"name\":\"{name}\",\"unavailable\":false,\"channels\":[],\"roles\":[],\"emojis\":[],\"features\":[],\"members\":[]}}");

	public static ReceivedGatewayMessage GuildUpdate(ulong id = 100, string name = "UpdatedGuild") =>
		Dispatch("GUILD_UPDATE",
			$"{{\"id\":\"{id}\",\"name\":\"{name}\",\"unavailable\":false,\"channels\":[],\"roles\":[],\"emojis\":[],\"features\":[],\"members\":[]}}");

	public static ReceivedGatewayMessage GuildDelete(ulong id = 100, bool unavailable = false) =>
		Dispatch("GUILD_DELETE", $"{{\"id\":\"{id}\",\"unavailable\":{(unavailable ? "true" : "false")}}}");

	public static ReceivedGatewayMessage GuildMemberAdd(ulong guildId = 100, ulong userId = 42, string username = "joiner") =>
		Dispatch("GUILD_MEMBER_ADD",
			$"{{\"guild_id\":\"{guildId}\","
			+ $"\"user\":{{\"id\":\"{userId}\",\"username\":\"{username}\",\"discriminator\":\"0001\"}},"
			+ "\"roles\":[],\"joined_at\":\"2024-01-01T00:00:00+00:00\"}");

	public static ReceivedGatewayMessage GuildMemberRemove(ulong guildId = 100, ulong userId = 42, string username = "leaver") =>
		Dispatch("GUILD_MEMBER_REMOVE",
			$"{{\"guild_id\":\"{guildId}\","
			+ $"\"user\":{{\"id\":\"{userId}\",\"username\":\"{username}\",\"discriminator\":\"0001\"}}}}");

	public static ReceivedGatewayMessage GuildMemberUpdate(ulong guildId = 100, ulong userId = 42, string nickname = "renamed") =>
		Dispatch("GUILD_MEMBER_UPDATE",
			$"{{\"guild_id\":\"{guildId}\","
			+ $"\"user\":{{\"id\":\"{userId}\",\"username\":\"u\",\"discriminator\":\"0001\"}},"
			+ $"\"nick\":\"{nickname}\",\"roles\":[],\"joined_at\":\"2024-01-01T00:00:00+00:00\"}}");

	public static ReceivedGatewayMessage GuildRoleCreate(ulong guildId = 100, ulong roleId = 500, string name = "Mods") =>
		Dispatch("GUILD_ROLE_CREATE",
			$"{{\"guild_id\":\"{guildId}\","
			+ $"\"role\":{{\"id\":\"{roleId}\",\"name\":\"{name}\",\"color\":0,\"hoist\":false,\"position\":1,\"permissions\":\"0\",\"managed\":false,\"mentionable\":false}}}}");

	public static ReceivedGatewayMessage GuildRoleUpdate(ulong guildId = 100, ulong roleId = 500, string name = "Renamed") =>
		Dispatch("GUILD_ROLE_UPDATE",
			$"{{\"guild_id\":\"{guildId}\","
			+ $"\"role\":{{\"id\":\"{roleId}\",\"name\":\"{name}\",\"color\":0,\"hoist\":false,\"position\":1,\"permissions\":\"0\",\"managed\":false,\"mentionable\":false}}}}");

	public static ReceivedGatewayMessage GuildRoleDelete(ulong guildId = 100, ulong roleId = 500) =>
		Dispatch("GUILD_ROLE_DELETE", $"{{\"guild_id\":\"{guildId}\",\"role_id\":\"{roleId}\"}}");

	public static ReceivedGatewayMessage GuildBanAdd(ulong guildId = 100, ulong userId = 42) =>
		Dispatch("GUILD_BAN_ADD",
			$"{{\"guild_id\":\"{guildId}\","
			+ $"\"user\":{{\"id\":\"{userId}\",\"username\":\"banned\",\"discriminator\":\"0001\"}}}}");

	public static ReceivedGatewayMessage GuildBanRemove(ulong guildId = 100, ulong userId = 42) =>
		Dispatch("GUILD_BAN_REMOVE",
			$"{{\"guild_id\":\"{guildId}\","
			+ $"\"user\":{{\"id\":\"{userId}\",\"username\":\"unbanned\",\"discriminator\":\"0001\"}}}}");

	public static ReceivedGatewayMessage MessageDeleteBulk(ulong channelId = 200, ulong? guildId = 100, params ulong[] ids)
	{
		var idsJson = string.Join(",", ids.Select(i => $"\"{i}\""));
		var guildField = guildId.HasValue ? $",\"guild_id\":\"{guildId.Value}\"" : string.Empty;
		return Dispatch("MESSAGE_DELETE_BULK",
			$"{{\"ids\":[{idsJson}],\"channel_id\":\"{channelId}\"{guildField}}}");
	}

	public static ReceivedGatewayMessage MessageReactionRemoveAll(ulong channelId = 200, ulong messageId = 300, ulong? guildId = 100)
	{
		var guildField = guildId.HasValue ? $",\"guild_id\":\"{guildId.Value}\"" : string.Empty;
		return Dispatch("MESSAGE_REACTION_REMOVE_ALL",
			$"{{\"channel_id\":\"{channelId}\",\"message_id\":\"{messageId}\"{guildField}}}");
	}

	public static ReceivedGatewayMessage MessageReactionRemoveEmoji(ulong channelId = 200, ulong messageId = 300, ulong? guildId = 100, string emojiName = "smile") =>
		Dispatch("MESSAGE_REACTION_REMOVE_EMOJI",
			$"{{\"channel_id\":\"{channelId}\",\"message_id\":\"{messageId}\","
			+ (guildId.HasValue ? $"\"guild_id\":\"{guildId.Value}\"," : string.Empty)
			+ $"\"emoji\":{{\"id\":null,\"name\":\"{emojiName}\"}}}}");

	public static ReceivedGatewayMessage UserUpdate(ulong userId = 42, string username = "selfbot") =>
		Dispatch("USER_UPDATE",
			$"{{\"id\":\"{userId}\",\"username\":\"{username}\",\"discriminator\":\"0001\"}}");

	public static ReceivedGatewayMessage ChannelPinsUpdate(ulong channelId = 200, ulong? guildId = 100, string? lastPinTimestamp = "2024-06-01T00:00:00+00:00")
	{
		var guildField = guildId.HasValue ? $"\"guild_id\":\"{guildId.Value}\"," : string.Empty;
		var tsField = lastPinTimestamp is not null ? $",\"last_pin_timestamp\":\"{lastPinTimestamp}\"" : string.Empty;
		return Dispatch("CHANNEL_PINS_UPDATE", $"{{{guildField}\"channel_id\":\"{channelId}\"{tsField}}}");
	}

	public static ReceivedGatewayMessage WebhooksUpdate(ulong guildId = 100, ulong channelId = 200) =>
		Dispatch("WEBHOOKS_UPDATE", $"{{\"guild_id\":\"{guildId}\",\"channel_id\":\"{channelId}\"}}");

	public static ReceivedGatewayMessage InviteCreate(string code = "invcode", ulong channelId = 200, ulong? guildId = 100, ulong? inviterId = 42) =>
		Dispatch("INVITE_CREATE",
			$"{{\"code\":\"{code}\",\"channel_id\":\"{channelId}\","
			+ (guildId.HasValue ? $"\"guild_id\":\"{guildId.Value}\"," : string.Empty)
			+ (inviterId.HasValue ? $"\"inviter\":{{\"id\":\"{inviterId.Value}\",\"username\":\"inviter\",\"discriminator\":\"0001\"}}," : string.Empty)
			+ "\"created_at\":\"2024-01-01T00:00:00+00:00\",\"max_age\":3600,\"max_uses\":5,\"temporary\":false,\"uses\":0}");

	public static ReceivedGatewayMessage InviteDelete(string code = "invcode", ulong channelId = 200, ulong? guildId = 100) =>
		Dispatch("INVITE_DELETE",
			$"{{\"code\":\"{code}\",\"channel_id\":\"{channelId}\""
			+ (guildId.HasValue ? $",\"guild_id\":\"{guildId.Value}\"" : string.Empty)
			+ "}");

	/// <summary>Thread channel payload (type 11 = public thread).</summary>
	private static string ThreadChannelJson(ulong id, ulong guildId, ulong parentId, int type = 11, string name = "thread") =>
		$"{{\"id\":\"{id}\",\"type\":{type},\"guild_id\":\"{guildId}\",\"parent_id\":\"{parentId}\",\"name\":\"{name}\"}}";

	public static ReceivedGatewayMessage ThreadCreate(ulong id = 400, ulong guildId = 100, ulong parentId = 200, int type = 11) =>
		Dispatch("THREAD_CREATE", ThreadChannelJson(id, guildId, parentId, type));

	public static ReceivedGatewayMessage ThreadUpdate(ulong id = 400, ulong guildId = 100, ulong parentId = 200, int type = 11, string name = "renamed") =>
		Dispatch("THREAD_UPDATE", ThreadChannelJson(id, guildId, parentId, type, name));

	public static ReceivedGatewayMessage ThreadDelete(ulong id = 400, ulong guildId = 100, ulong parentId = 200, int type = 11) =>
		Dispatch("THREAD_DELETE", $"{{\"id\":\"{id}\",\"guild_id\":\"{guildId}\",\"parent_id\":\"{parentId}\",\"type\":{type}}}");

	public static ReceivedGatewayMessage ThreadListSync(ulong guildId = 100, params ulong[] threadIds)
	{
		var threadsJson = string.Join(",", threadIds.Select(id => ThreadChannelJson(id, guildId, parentId: 200)));
		return Dispatch("THREAD_LIST_SYNC",
			$"{{\"guild_id\":\"{guildId}\",\"threads\":[{threadsJson}],\"members\":[]}}");
	}

	public static ReceivedGatewayMessage ThreadMemberUpdate(ulong threadId = 400, ulong userId = 42, ulong guildId = 100) =>
		Dispatch("THREAD_MEMBER_UPDATE",
			$"{{\"id\":\"{threadId}\",\"user_id\":\"{userId}\",\"guild_id\":\"{guildId}\",\"join_timestamp\":\"2024-01-01T00:00:00+00:00\",\"flags\":0}}");

	public static ReceivedGatewayMessage ThreadMembersUpdate(ulong threadId = 400, ulong guildId = 100, int memberCount = 5, ulong[]? addedIds = null, ulong[]? removedIds = null)
	{
		var addedJson = addedIds is null ? "[]" : "[" + string.Join(",", addedIds.Select(id => $"{{\"user_id\":\"{id}\"}}")) + "]";
		var removedJson = removedIds is null ? "[]" : "[" + string.Join(",", removedIds.Select(id => $"\"{id}\"")) + "]";
		return Dispatch("THREAD_MEMBERS_UPDATE",
			$"{{\"id\":\"{threadId}\",\"guild_id\":\"{guildId}\",\"member_count\":{memberCount},\"added_members\":{addedJson},\"removed_member_ids\":{removedJson}}}");
	}

	private static string StageInstanceJson(ulong id, ulong guildId, ulong channelId, string topic = "Stage") =>
		$"{{\"id\":\"{id}\",\"guild_id\":\"{guildId}\",\"channel_id\":\"{channelId}\",\"topic\":\"{topic}\",\"privacy_level\":2,\"discoverable_disabled\":false}}";

	public static ReceivedGatewayMessage StageInstanceCreate(ulong id = 500, ulong guildId = 100, ulong channelId = 200) =>
		Dispatch("STAGE_INSTANCE_CREATE", StageInstanceJson(id, guildId, channelId));

	public static ReceivedGatewayMessage StageInstanceUpdate(ulong id = 500, ulong guildId = 100, ulong channelId = 200, string topic = "Renamed") =>
		Dispatch("STAGE_INSTANCE_UPDATE", StageInstanceJson(id, guildId, channelId, topic));

	public static ReceivedGatewayMessage StageInstanceDelete(ulong id = 500, ulong guildId = 100, ulong channelId = 200) =>
		Dispatch("STAGE_INSTANCE_DELETE", StageInstanceJson(id, guildId, channelId));

	private static string AutoModRuleJson(ulong id, ulong guildId, string name = "no-spam") =>
		$"{{\"id\":\"{id}\",\"guild_id\":\"{guildId}\",\"name\":\"{name}\",\"creator_id\":\"1\",\"event_type\":1,\"trigger_type\":1,\"actions\":[],\"enabled\":true,\"exempt_roles\":[],\"exempt_channels\":[]}}";

	public static ReceivedGatewayMessage AutoModRuleCreate(ulong id = 600, ulong guildId = 100) =>
		Dispatch("AUTO_MODERATION_RULE_CREATE", AutoModRuleJson(id, guildId));

	public static ReceivedGatewayMessage AutoModRuleUpdate(ulong id = 600, ulong guildId = 100, string name = "renamed") =>
		Dispatch("AUTO_MODERATION_RULE_UPDATE", AutoModRuleJson(id, guildId, name));

	public static ReceivedGatewayMessage AutoModRuleDelete(ulong id = 600, ulong guildId = 100) =>
		Dispatch("AUTO_MODERATION_RULE_DELETE", AutoModRuleJson(id, guildId));

	public static ReceivedGatewayMessage AutoModActionExecution(ulong guildId = 100, ulong ruleId = 600, ulong userId = 42, ulong? channelId = 200, string matchedContent = "spam") =>
		Dispatch("AUTO_MODERATION_ACTION_EXECUTION",
			$"{{\"guild_id\":\"{guildId}\",\"rule_id\":\"{ruleId}\",\"user_id\":\"{userId}\","
			+ (channelId.HasValue ? $"\"channel_id\":\"{channelId.Value}\"," : string.Empty)
			+ "\"action\":{\"type\":1},\"rule_trigger_type\":1,"
			+ $"\"matched_content\":\"{matchedContent}\"}}");

	private static string IntegrationJson(ulong id, ulong guildId, string name = "MyApp") =>
		$"{{\"id\":\"{id}\",\"guild_id\":\"{guildId}\",\"name\":\"{name}\",\"type\":\"discord\",\"enabled\":true,"
		+ "\"account\":{\"id\":\"acc\",\"name\":\"acc\"}}";

	public static ReceivedGatewayMessage IntegrationCreate(ulong id = 700, ulong guildId = 100) =>
		Dispatch("INTEGRATION_CREATE", IntegrationJson(id, guildId));

	public static ReceivedGatewayMessage IntegrationUpdate(ulong id = 700, ulong guildId = 100, string name = "renamed") =>
		Dispatch("INTEGRATION_UPDATE", IntegrationJson(id, guildId, name));

	public static ReceivedGatewayMessage IntegrationDelete(ulong id = 700, ulong guildId = 100, ulong? applicationId = 901) =>
		Dispatch("INTEGRATION_DELETE",
			$"{{\"id\":\"{id}\",\"guild_id\":\"{guildId}\""
			+ (applicationId.HasValue ? $",\"application_id\":\"{applicationId.Value}\"" : string.Empty)
			+ "}");

	private static string ScheduledEventJson(ulong id, ulong guildId, string name = "Meetup") =>
		$"{{\"id\":\"{id}\",\"guild_id\":\"{guildId}\",\"channel_id\":null,\"name\":\"{name}\",\"scheduled_start_time\":\"2024-12-01T00:00:00+00:00\",\"privacy_level\":2,\"status\":1,\"entity_type\":3}}";

	public static ReceivedGatewayMessage ScheduledEventCreate(ulong id = 800, ulong guildId = 100) =>
		Dispatch("GUILD_SCHEDULED_EVENT_CREATE", ScheduledEventJson(id, guildId));

	public static ReceivedGatewayMessage ScheduledEventUpdate(ulong id = 800, ulong guildId = 100, string name = "renamed") =>
		Dispatch("GUILD_SCHEDULED_EVENT_UPDATE", ScheduledEventJson(id, guildId, name));

	public static ReceivedGatewayMessage ScheduledEventDelete(ulong id = 800, ulong guildId = 100) =>
		Dispatch("GUILD_SCHEDULED_EVENT_DELETE", ScheduledEventJson(id, guildId));

	public static ReceivedGatewayMessage ScheduledEventUserAdd(ulong eventId = 800, ulong userId = 42, ulong guildId = 100) =>
		Dispatch("GUILD_SCHEDULED_EVENT_USER_ADD",
			$"{{\"guild_scheduled_event_id\":\"{eventId}\",\"user_id\":\"{userId}\",\"guild_id\":\"{guildId}\"}}");

	public static ReceivedGatewayMessage ScheduledEventUserRemove(ulong eventId = 800, ulong userId = 42, ulong guildId = 100) =>
		Dispatch("GUILD_SCHEDULED_EVENT_USER_REMOVE",
			$"{{\"guild_scheduled_event_id\":\"{eventId}\",\"user_id\":\"{userId}\",\"guild_id\":\"{guildId}\"}}");

	private static string EntitlementJson(ulong id, ulong skuId = 1234, ulong appId = 5678) =>
		$"{{\"id\":\"{id}\",\"sku_id\":\"{skuId}\",\"application_id\":\"{appId}\",\"type\":8,\"deleted\":false}}";

	public static ReceivedGatewayMessage EntitlementCreate(ulong id = 900) =>
		Dispatch("ENTITLEMENT_CREATE", EntitlementJson(id));

	public static ReceivedGatewayMessage EntitlementUpdate(ulong id = 900) =>
		Dispatch("ENTITLEMENT_UPDATE", EntitlementJson(id));

	public static ReceivedGatewayMessage EntitlementDelete(ulong id = 900) =>
		Dispatch("ENTITLEMENT_DELETE", EntitlementJson(id));

	public static ReceivedGatewayMessage AuditLogEntryCreate(ulong id = 1000, ulong guildId = 100, int actionType = 1) =>
		Dispatch("GUILD_AUDIT_LOG_ENTRY_CREATE",
			$"{{\"id\":\"{id}\",\"action_type\":{actionType},\"guild_id\":\"{guildId}\"}}");

	public static ReceivedGatewayMessage GuildEmojisUpdate(ulong guildId = 100, params (ulong id, string name)[] emojis)
	{
		var emojisJson = string.Join(",", emojis.Select(e => $"{{\"id\":\"{e.id}\",\"name\":\"{e.name}\"}}"));
		return Dispatch("GUILD_EMOJIS_UPDATE", $"{{\"guild_id\":\"{guildId}\",\"emojis\":[{emojisJson}]}}");
	}

	public static ReceivedGatewayMessage GuildStickersUpdate(ulong guildId = 100, params (ulong id, string name)[] stickers)
	{
		var stickersJson = string.Join(",", stickers.Select(s => $"{{\"id\":\"{s.id}\",\"name\":\"{s.name}\",\"description\":\"\",\"tags\":\"\",\"type\":2,\"format_type\":1,\"available\":true}}"));
		return Dispatch("GUILD_STICKERS_UPDATE", $"{{\"guild_id\":\"{guildId}\",\"stickers\":[{stickersJson}]}}");
	}

	public static ReceivedGatewayMessage GuildIntegrationsUpdate(ulong guildId = 100) =>
		Dispatch("GUILD_INTEGRATIONS_UPDATE", $"{{\"guild_id\":\"{guildId}\"}}");

	public static ReceivedGatewayMessage PresenceUpdate(ulong userId = 42, ulong? guildId = 100, string status = "online") =>
		Dispatch("PRESENCE_UPDATE",
			$"{{\"user\":{{\"id\":\"{userId}\"}},"
			+ (guildId.HasValue ? $"\"guild_id\":\"{guildId.Value}\"," : string.Empty)
			+ $"\"status\":\"{status}\",\"activities\":[],\"client_status\":{{}}}}");

	public static ReceivedGatewayMessage GuildMembersChunk(ulong guildId = 100, int chunkIndex = 0, int chunkCount = 1, string? nonce = null, params ulong[] userIds)
	{
		var membersJson = string.Join(",", userIds.Select(uid =>
			$"{{\"user\":{{\"id\":\"{uid}\",\"username\":\"u\",\"discriminator\":\"0001\"}},\"roles\":[],\"joined_at\":\"2024-01-01T00:00:00+00:00\"}}"));
		var nonceField = nonce is null ? string.Empty : $",\"nonce\":\"{nonce}\"";
		return Dispatch("GUILD_MEMBERS_CHUNK",
			$"{{\"guild_id\":\"{guildId}\",\"members\":[{membersJson}],\"chunk_index\":{chunkIndex},\"chunk_count\":{chunkCount}{nonceField}}}");
	}

	public static ReceivedGatewayMessage MessagePollVoteAdd(ulong userId = 42, ulong channelId = 200, ulong messageId = 300, ulong? guildId = 100, int answerId = 1) =>
		Dispatch("MESSAGE_POLL_VOTE_ADD",
			$"{{\"user_id\":\"{userId}\",\"channel_id\":\"{channelId}\",\"message_id\":\"{messageId}\","
			+ (guildId.HasValue ? $"\"guild_id\":\"{guildId.Value}\"," : string.Empty)
			+ $"\"answer_id\":{answerId}}}");

	public static ReceivedGatewayMessage MessagePollVoteRemove(ulong userId = 42, ulong channelId = 200, ulong messageId = 300, ulong? guildId = 100, int answerId = 1) =>
		Dispatch("MESSAGE_POLL_VOTE_REMOVE",
			$"{{\"user_id\":\"{userId}\",\"channel_id\":\"{channelId}\",\"message_id\":\"{messageId}\","
			+ (guildId.HasValue ? $"\"guild_id\":\"{guildId.Value}\"," : string.Empty)
			+ $"\"answer_id\":{answerId}}}");

	private static string SubscriptionJson(ulong id, ulong userId = 42) =>
		$"{{\"id\":\"{id}\",\"user_id\":\"{userId}\",\"sku_ids\":[],\"entitlement_ids\":[],"
		+ "\"current_period_start\":\"2024-01-01T00:00:00+00:00\","
		+ "\"current_period_end\":\"2024-02-01T00:00:00+00:00\",\"status\":0}";

	public static ReceivedGatewayMessage SubscriptionCreate(ulong id = 1300) =>
		Dispatch("SUBSCRIPTION_CREATE", SubscriptionJson(id));

	public static ReceivedGatewayMessage SubscriptionUpdate(ulong id = 1300) =>
		Dispatch("SUBSCRIPTION_UPDATE", SubscriptionJson(id));

	public static ReceivedGatewayMessage SubscriptionDelete(ulong id = 1300) =>
		Dispatch("SUBSCRIPTION_DELETE", SubscriptionJson(id));

	public static ReceivedGatewayMessage ChannelCreate(ulong id = 200, ulong guildId = 100, int type = 0) =>
		Dispatch("CHANNEL_CREATE", $"{{\"id\":\"{id}\",\"type\":{type},\"guild_id\":\"{guildId}\",\"name\":\"general\"}}");

	public static ReceivedGatewayMessage ChannelUpdate(ulong id = 200, ulong guildId = 100, int type = 0, string name = "renamed") =>
		Dispatch("CHANNEL_UPDATE", $"{{\"id\":\"{id}\",\"type\":{type},\"guild_id\":\"{guildId}\",\"name\":\"{name}\"}}");

	public static ReceivedGatewayMessage ChannelDelete(ulong id = 200, ulong guildId = 100, int type = 0) =>
		Dispatch("CHANNEL_DELETE", $"{{\"id\":\"{id}\",\"type\":{type},\"guild_id\":\"{guildId}\"}}");

	public static ReceivedGatewayMessage MessageDelete(ulong id = 300, ulong channelId = 200, ulong? guildId = 100)
	{
		var guildField = guildId.HasValue ? $",\"guild_id\":\"{guildId.Value}\"" : string.Empty;
		return Dispatch("MESSAGE_DELETE", $"{{\"id\":\"{id}\",\"channel_id\":\"{channelId}\"{guildField}}}");
	}

	public static ReceivedGatewayMessage TypingStart(ulong userId = 42, ulong channelId = 200, ulong guildId = 100, long timestamp = 1700000000)
	{
		// Embed member.user so the dispatcher's GetUserAsync doesn't fall back to a real REST call.
		var member = $"\"member\":{{\"user\":{{\"id\":\"{userId}\",\"username\":\"u\",\"discriminator\":\"0000\"}},\"joined_at\":\"2024-01-01T00:00:00+00:00\",\"roles\":[]}}";
		return Dispatch("TYPING_START",
			$"{{\"user_id\":\"{userId}\",\"channel_id\":\"{channelId}\",\"guild_id\":\"{guildId}\",\"timestamp\":{timestamp},{member}}}");
	}

	public static ReceivedGatewayMessage MessageReactionAdd(ulong userId = 42, ulong channelId = 200, ulong guildId = 100, ulong messageId = 300, string emojiName = "smile")
	{
		var member = $"\"member\":{{\"user\":{{\"id\":\"{userId}\",\"username\":\"u\",\"discriminator\":\"0000\"}},\"joined_at\":\"2024-01-01T00:00:00+00:00\",\"roles\":[]}}";
		return Dispatch("MESSAGE_REACTION_ADD",
			$"{{\"user_id\":\"{userId}\",\"channel_id\":\"{channelId}\",\"guild_id\":\"{guildId}\",\"message_id\":\"{messageId}\",\"emoji\":{{\"id\":null,\"name\":\"{emojiName}\"}},{member}}}");
	}

	public static ReceivedGatewayMessage MessageReactionRemove(ulong userId = 42, ulong channelId = 200, ulong guildId = 100, ulong messageId = 300, string emojiName = "smile") =>
		Dispatch("MESSAGE_REACTION_REMOVE",
			$"{{\"user_id\":\"{userId}\",\"channel_id\":\"{channelId}\",\"guild_id\":\"{guildId}\",\"message_id\":\"{messageId}\",\"emoji\":{{\"id\":null,\"name\":\"{emojiName}\"}}}}");

	public static ReceivedGatewayMessage InteractionCreate(int type, int? commandType = null, ulong? channelId = 200, ulong applicationId = 900)
	{
		// User/Message context-menu commands require a target_id + a matching entry in
		// resolved.users / resolved.messages — the context constructors throw otherwise.
		// Autocomplete requires at least one option (focused, ideally).
		var data = type switch
		{
			2 when commandType == 2 =>
				"\"data\":{\"id\":\"1\",\"name\":\"cmd\",\"type\":2,\"target_id\":\"77\","
				+ "\"resolved\":{\"users\":{\"77\":{\"id\":\"77\",\"username\":\"target\",\"discriminator\":\"0001\"}}}}",
			2 when commandType == 3 =>
				"\"data\":{\"id\":\"1\",\"name\":\"cmd\",\"type\":3,\"target_id\":\"88\","
				+ "\"resolved\":{\"messages\":{\"88\":{\"id\":\"88\",\"channel_id\":\"200\",\"content\":\"hi\","
				+ "\"author\":{\"id\":\"77\",\"username\":\"target\",\"discriminator\":\"0001\"},"
				+ "\"timestamp\":\"2024-01-01T00:00:00+00:00\",\"tts\":false,\"mention_everyone\":false,"
				+ "\"mentions\":[],\"mention_roles\":[],\"attachments\":[],\"embeds\":[],\"pinned\":false,\"type\":0}}}}",
			2 => commandType.HasValue
				? $"\"data\":{{\"id\":\"1\",\"name\":\"cmd\",\"type\":{commandType.Value}}}"
				: "\"data\":{\"id\":\"1\",\"name\":\"cmd\",\"type\":1}",
			3 => "\"data\":{\"custom_id\":\"btn\",\"component_type\":2}",
			4 =>
				"\"data\":{\"id\":\"1\",\"name\":\"cmd\",\"type\":1,"
				+ "\"options\":[{\"name\":\"query\",\"type\":3,\"value\":\"hel\",\"focused\":true}]}",
			5 => "\"data\":{\"custom_id\":\"modal\",\"components\":[]}",
			_ => "\"data\":null"
		};
		var channel = channelId.HasValue ? $",\"channel_id\":\"{channelId.Value}\"" : string.Empty;
		return Dispatch("INTERACTION_CREATE",
			$"{{\"id\":\"50\",\"application_id\":\"{applicationId}\",\"type\":{type},\"token\":\"tok\",\"version\":1{channel},{data}}}");
	}
}

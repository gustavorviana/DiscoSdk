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

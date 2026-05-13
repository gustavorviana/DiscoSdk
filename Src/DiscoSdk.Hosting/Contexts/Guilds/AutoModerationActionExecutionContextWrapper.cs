using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models;
using DiscoSdk.Models.AutoModeration;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class AutoModerationActionExecutionContextWrapper(DiscordClient client,
	IGuild guild,
	Snowflake ruleId,
	AutoModerationAction action,
	Snowflake userId,
	Snowflake? channelId,
	string? matchedContent) : ContextWrapper(client), IAutoModerationActionExecutionContext
{
	public IGuild Guild => guild;
	public Snowflake RuleId => ruleId;
	public AutoModerationAction Action => action;
	public Snowflake UserId => userId;
	public Snowflake? ChannelId => channelId;
	public string? MatchedContent => matchedContent;
}

using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models;
using DiscoSdk.Models.AutoModeration;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class AutoModerationRuleContextWrapper(DiscordClient client, AutoModerationRule rule, IGuild guild)
	: ContextWrapper(client), IAutoModerationRuleContext
{
	public AutoModerationRule Rule => rule;
	public IGuild Guild => guild;
}

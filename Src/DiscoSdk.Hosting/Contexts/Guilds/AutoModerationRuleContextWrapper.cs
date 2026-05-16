using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.AutoModeration;

namespace DiscoSdk.Hosting.Contexts.Guilds;

internal class AutoModerationRuleContextWrapper(DiscordClient client, AutoModerationRule rule, IGuild guild)
	: ContextWrapper(client), IAutoModerationRuleContext
{
	private IAutoModerationRule? _wrapped;
	public IAutoModerationRule Rule => _wrapped ??= new AutoModerationRuleWrapper(client, rule);
	public IGuild Guild => guild;
}

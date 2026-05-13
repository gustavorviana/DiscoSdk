using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="IEditGuildOnboardingAction"/>.
/// </summary>
internal sealed class EditGuildOnboardingAction(DiscordClient client, Snowflake guildId)
	: RestAction<IGuildOnboarding>, IEditGuildOnboardingAction
{
	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
	private readonly Dictionary<string, object?> _changes = [];

	public IEditGuildOnboardingAction SetPrompts(params OnboardingPrompt[] prompts)
	{
		_changes["prompts"] = prompts;
		return this;
	}

	public IEditGuildOnboardingAction SetDefaultChannelIds(params Snowflake[] channelIds)
	{
		_changes["default_channel_ids"] = channelIds?.Select(c => c.ToString()).ToArray();
		return this;
	}

	public IEditGuildOnboardingAction SetEnabled(bool enabled)
	{
		_changes["enabled"] = enabled;
		return this;
	}

	public IEditGuildOnboardingAction SetMode(OnboardingMode mode)
	{
		_changes["mode"] = (int)mode;
		return this;
	}

	public override async Task<IGuildOnboarding> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		var onboarding = await _client.GuildTemplateClient.ModifyOnboardingAsync(guildId, _changes, cancellationToken);
		return new GuildOnboardingWrapper(_client, onboarding);
	}
}

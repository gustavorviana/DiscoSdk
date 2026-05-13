using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Wraps a <see cref="GuildOnboarding"/> model and exposes the operations available on it.
/// </summary>
internal sealed class GuildOnboardingWrapper(DiscordClient client, GuildOnboarding model) : IGuildOnboarding
{
	private readonly GuildOnboarding _model = model ?? throw new ArgumentNullException(nameof(model));
	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
	private IReadOnlyList<IOnboardingPrompt>? _prompts;

	public Snowflake GuildId => _model.GuildId;
	public IReadOnlyList<IOnboardingPrompt> Prompts => _prompts ??=
		_model.Prompts.Select(p => (IOnboardingPrompt)new OnboardingPromptWrapper(_client, p)).ToList().AsReadOnly();
	public IReadOnlyList<Snowflake> DefaultChannelIds => _model.DefaultChannelIds;
	public bool Enabled => _model.Enabled;
	public OnboardingMode Mode => _model.Mode;

	public IEditGuildOnboardingAction Modify() => new EditGuildOnboardingAction(_client, _model.GuildId);
}

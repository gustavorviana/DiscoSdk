using DiscoSdk.Models;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Hosting.Wrappers;

internal sealed class OnboardingPromptWrapper(DiscordClient client, OnboardingPrompt model) : IOnboardingPrompt
{
	private readonly OnboardingPrompt _model = model ?? throw new ArgumentNullException(nameof(model));
	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
	private IReadOnlyList<IOnboardingPromptOption>? _options;

	public Snowflake Id => _model.Id;
	public OnboardingPromptType Type => _model.Type;
	public IReadOnlyList<IOnboardingPromptOption> Options => _options ??=
		_model.Options.Select(o => (IOnboardingPromptOption)new OnboardingPromptOptionWrapper(_client, o)).ToList().AsReadOnly();
	public string Title => _model.Title;
	public bool SingleSelect => _model.SingleSelect;
	public bool Required => _model.Required;
	public bool InOnboarding => _model.InOnboarding;
}

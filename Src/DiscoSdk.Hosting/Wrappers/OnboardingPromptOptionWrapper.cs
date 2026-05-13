using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Wrappers;

internal sealed class OnboardingPromptOptionWrapper(DiscordClient client, OnboardingPromptOption model) : IOnboardingPromptOption
{
	private readonly OnboardingPromptOption _model = model ?? throw new ArgumentNullException(nameof(model));
	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
	private IEmoji? _emoji;

	public Snowflake Id => _model.Id;
	public IReadOnlyList<Snowflake> ChannelIds => _model.ChannelIds;
	public IReadOnlyList<Snowflake> RoleIds => _model.RoleIds;
	public IEmoji? Emoji => _emoji ??= _model.Emoji is { } e ? new EmojiWrapper(_client, e, guild: null) : null;
	public string Title => _model.Title;
	public string? Description => _model.Description;
}

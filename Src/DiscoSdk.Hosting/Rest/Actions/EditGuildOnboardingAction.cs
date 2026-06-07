using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="IEditGuildOnboardingAction"/>.
/// </summary>
internal sealed class EditGuildOnboardingAction(DiscordClient client, Snowflake guildId)
	: RestAction<IGuildOnboarding>, IEditGuildOnboardingAction
{
	private const int MaxPromptCount = 50;

	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
	private readonly Dictionary<string, object?> _changes = [];
	private string? _reason;

	public IEditGuildOnboardingAction SetPrompts(params OnboardingPrompt[] prompts)
	{
		ArgumentNullException.ThrowIfNull(prompts);
		if (prompts.Length > MaxPromptCount)
			throw new ArgumentOutOfRangeException(nameof(prompts), $"Onboarding cannot have more than {MaxPromptCount} prompts.");
		_changes["prompts"] = new List<OnboardingPrompt>(prompts);
		return this;
	}

	public IEditGuildOnboardingAction SetPrompts(params OnboardingPromptBuilder[] prompts)
	{
		ArgumentNullException.ThrowIfNull(prompts);
		var built = prompts.Select(p => (p ?? throw new ArgumentException("Prompt builder cannot be null.", nameof(prompts))).Build()).ToArray();
		return SetPrompts(built);
	}

	public IEditGuildOnboardingAction AddPrompt(Action<OnboardingPromptBuilder> configure)
	{
		ArgumentNullException.ThrowIfNull(configure);
		var b = new OnboardingPromptBuilder();
		configure(b);
		var prompt = b.Build();

		if (_changes.TryGetValue("prompts", out var existing) && existing is List<OnboardingPrompt> list)
		{
			if (list.Count >= MaxPromptCount)
				throw new InvalidOperationException($"Onboarding cannot have more than {MaxPromptCount} prompts.");
			list.Add(prompt);
		}
		else
		{
			_changes["prompts"] = new List<OnboardingPrompt> { prompt };
		}
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

	public IEditGuildOnboardingAction WithReason(string reason)
	{
		_reason = AuditLogReason.Validate(reason);
		return this;
	}

	public override async Task<IGuildOnboarding> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		if (_changes.Count == 0)
			throw new InvalidOperationException("EditGuildOnboarding requires at least one Set* call before ExecuteAsync.");

		var onboarding = await _client.GuildTemplateClient.ModifyOnboardingAsync(guildId, _changes, _reason, cancellationToken);
		return new GuildOnboardingWrapper(_client, onboarding);
	}
}

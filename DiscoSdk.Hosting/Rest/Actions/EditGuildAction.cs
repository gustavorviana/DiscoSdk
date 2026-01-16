using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="IEditGuildAction"/> for editing Discord guilds.
/// </summary>
internal class EditGuildAction : RestAction<IGuild>, IEditGuildAction
{
	private readonly DiscordClient _client;
	private readonly IGuild _guild;
	private string? _name;
	private VerificationLevel? _verificationLevel;
	private DefaultMessageNotificationLevel? _defaultMessageNotifications;
	private ExplicitContentFilterLevel? _explicitContentFilter;
	private Snowflake? _afkChannelId;
	private bool _afkChannelIdSet;
	private int? _afkTimeout;
	private DiscordImage? _icon;
	private DiscordImage? _splash;
	private DiscordImage? _discoverySplash;
	private DiscordImage? _banner;
	private Snowflake? _systemChannelId;
	private bool _systemChannelIdSet;
	private SystemChannelFlags? _systemChannelFlags;
	private Snowflake? _rulesChannelId;
	private bool _rulesChannelIdSet;
	private Snowflake? _publicUpdatesChannelId;
	private bool _publicUpdatesChannelIdSet;
	private string? _preferredLocale;
	private bool _preferredLocaleSet;
	private string? _description;
	private bool _descriptionSet;
	private bool? _premiumProgressBarEnabled;
	private Snowflake? _safetyAlertsChannelId;
	private bool _safetyAlertsChannelIdSet;

	public EditGuildAction(DiscordClient client, IGuild guild)
	{
		_client = client ?? throw new ArgumentNullException(nameof(client));
		_guild = guild ?? throw new ArgumentNullException(nameof(guild));
	}

	public IEditGuildAction SetName(string name)
	{
		_name = name;
		return this;
	}

	public IEditGuildAction SetVerificationLevel(VerificationLevel? level)
	{
		_verificationLevel = level;
		return this;
	}

	public IEditGuildAction SetDefaultMessageNotifications(DefaultMessageNotificationLevel? level)
	{
		_defaultMessageNotifications = level;
		return this;
	}

	public IEditGuildAction SetExplicitContentFilter(ExplicitContentFilterLevel? level)
	{
		_explicitContentFilter = level;
		return this;
	}

	public IEditGuildAction SetAfkChannelId(Snowflake? channelId)
	{
		_afkChannelId = channelId;
		_afkChannelIdSet = true;
		return this;
	}

	public IEditGuildAction SetAfkTimeout(int? timeout)
	{
		_afkTimeout = timeout;
		return this;
	}

	public IEditGuildAction SetIcon(DiscordImage? icon)
	{
		_icon = icon;
		return this;
	}

	public IEditGuildAction SetSplash(DiscordImage? splash)
	{
		_splash = splash;
		return this;
	}

	public IEditGuildAction SetDiscoverySplash(DiscordImage? discoverySplash)
	{
		_discoverySplash = discoverySplash;
		return this;
	}

	public IEditGuildAction SetBanner(DiscordImage? banner)
	{
		_banner = banner;
		return this;
	}

	public IEditGuildAction SetSystemChannelId(Snowflake? channelId)
	{
		_systemChannelId = channelId;
		_systemChannelIdSet = true;
		return this;
	}

	public IEditGuildAction SetSystemChannelFlags(SystemChannelFlags? flags)
	{
		_systemChannelFlags = flags;
		return this;
	}

	public IEditGuildAction SetRulesChannelId(Snowflake? channelId)
	{
		_rulesChannelId = channelId;
		_rulesChannelIdSet = true;
		return this;
	}

	public IEditGuildAction SetPublicUpdatesChannelId(Snowflake? channelId)
	{
		_publicUpdatesChannelId = channelId;
		_publicUpdatesChannelIdSet = true;
		return this;
	}

	public IEditGuildAction SetPreferredLocale(string? locale)
	{
		_preferredLocale = locale;
		_preferredLocaleSet = true;
		return this;
	}

	public IEditGuildAction SetDescription(string? description)
	{
		_description = description;
		_descriptionSet = true;
		return this;
	}

	public IEditGuildAction SetPremiumProgressBarEnabled(bool? enabled)
	{
		_premiumProgressBarEnabled = enabled;
		return this;
	}

	public IEditGuildAction SetSafetyAlertsChannelId(Snowflake? channelId)
	{
		_safetyAlertsChannelId = channelId;
		_safetyAlertsChannelIdSet = true;
		return this;
	}

	public override async Task<IGuild> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		var request = new Dictionary<string, object?>();

		if (_name != null)
			request["name"] = _name;

		if (_verificationLevel.HasValue)
			request["verification_level"] = (int)_verificationLevel.Value;

		if (_defaultMessageNotifications.HasValue)
			request["default_message_notifications"] = (int)_defaultMessageNotifications.Value;

		if (_explicitContentFilter.HasValue)
			request["explicit_content_filter"] = (int)_explicitContentFilter.Value;

		if (_afkChannelIdSet)
			request["afk_channel_id"] = _afkChannelId?.ToString();

		if (_afkTimeout.HasValue)
			request["afk_timeout"] = _afkTimeout.Value;

		if (_icon != null)
		{
			var base64 = _icon.ToBase64();
			request["icon"] = $"data:{_icon.ImageType};base64,{base64}";
		}

		if (_splash != null)
		{
			var base64 = _splash.ToBase64();
			request["splash"] = $"data:{_splash.ImageType};base64,{base64}";
		}

		if (_discoverySplash != null)
		{
			var base64 = _discoverySplash.ToBase64();
			request["discovery_splash"] = $"data:{_discoverySplash.ImageType};base64,{base64}";
		}

		if (_banner != null)
		{
			var base64 = _banner.ToBase64();
			request["banner"] = $"data:{_banner.ImageType};base64,{base64}";
		}

		if (_systemChannelIdSet)
			request["system_channel_id"] = _systemChannelId?.ToString();

		if (_systemChannelFlags.HasValue)
			request["system_channel_flags"] = (int)_systemChannelFlags.Value;

		if (_rulesChannelIdSet)
			request["rules_channel_id"] = _rulesChannelId?.ToString();

		if (_publicUpdatesChannelIdSet)
			request["public_updates_channel_id"] = _publicUpdatesChannelId?.ToString();

		if (_preferredLocaleSet)
			request["preferred_locale"] = _preferredLocale;

		if (_descriptionSet)
			request["description"] = _description;

		if (_premiumProgressBarEnabled.HasValue)
			request["premium_progress_bar_enabled"] = _premiumProgressBarEnabled.Value;

		if (_safetyAlertsChannelIdSet)
			request["safety_alerts_channel_id"] = _safetyAlertsChannelId?.ToString();

		var guild = await _client.GuildClient.EditAsync(_guild.Id, request, cancellationToken);

		// Update the wrapper if it's a GuildWrapper
		if (_guild is GuildWrapper wrapper)
		{
			wrapper.OnUpdate(guild);
			return wrapper;
		}

		return new GuildWrapper(guild, _client);
	}
}


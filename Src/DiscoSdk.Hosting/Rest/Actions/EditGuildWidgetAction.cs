using DiscoSdk.Models;
using DiscoSdk.Rest;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="IEditGuildWidgetAction"/> for editing a guild widget.
/// </summary>
internal class EditGuildWidgetAction : RestAction<IGuildWidget>, IEditGuildWidgetAction
{
	private readonly DiscordClient _client;
	private readonly IGuild _guild;
	private bool? _enabled;
	private Snowflake? _channelId;
	private bool _enabledSet;
	private bool _channelIdSet;
	private string? _reason;

	public EditGuildWidgetAction(DiscordClient client, IGuild guild)
	{
		_client = client ?? throw new ArgumentNullException(nameof(client));
		_guild = guild ?? throw new ArgumentNullException(nameof(guild));
	}

	/// <inheritdoc />
	public IEditGuildWidgetAction SetEnabled(bool enabled)
	{
		_enabled = enabled;
		_enabledSet = true;
		return this;
	}

	/// <inheritdoc />
	public IEditGuildWidgetAction SetChannelId(Snowflake? channelId)
	{
		_channelId = channelId;
		_channelIdSet = true;
		return this;
	}

	/// <inheritdoc />
	public IEditGuildWidgetAction WithReason(string reason)
	{
		_reason = AuditLogReason.Validate(reason);
		return this;
	}

	/// <inheritdoc />
	public override async Task<IGuildWidget> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		var request = new Dictionary<string, object?>();

		if (_enabledSet)
			request["enabled"] = _enabled;

		if (_channelIdSet)
			request["channel_id"] = _channelId?.ToString();

		return await _client.GuildClient.EditWidgetAsync(_guild.Id, request, _reason, cancellationToken);
	}
}

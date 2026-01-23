using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="IBanMemberAction"/> for banning Discord guild members.
/// </summary>
internal class BanMemberAction : RestAction, IBanMemberAction
{
	private readonly DiscordClient _client;
	private readonly Snowflake _guildId;
	private readonly Snowflake _userId;
	private int? _deleteMessageDays;
	private string? _reason;

	public BanMemberAction(DiscordClient client, Snowflake guildId, Snowflake userId, int deleteMessageDays = 0)
	{
		_client = client ?? throw new ArgumentNullException(nameof(client));
		_guildId = guildId;
		_userId = userId;
		_deleteMessageDays = deleteMessageDays;
	}

	public IBanMemberAction SetDeleteMessageDays(int days)
	{
		if (days < 0 || days > 7)
			throw new ArgumentOutOfRangeException(nameof(days), "Delete message days must be between 0 and 7.");

		_deleteMessageDays = days;
		return this;
	}

	public IBanMemberAction SetReason(string? reason)
	{
		_reason = reason;
		return this;
	}

	public override async Task ExecuteAsync(CancellationToken cancellationToken = default)
	{
		var request = new Dictionary<string, object?>();

		if (_deleteMessageDays.HasValue)
			request["delete_message_days"] = _deleteMessageDays.Value;

		if (_reason != null)
			request["reason"] = _reason;

		await _client.GuildClient.BanMemberAsync(_guildId, _userId, request, cancellationToken);
	}
}


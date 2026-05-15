using DiscoSdk.Hosting.Wrappers.Channels;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Requests.Channels;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="ICreateGroupDmAction"/>. Accumulates recipients (each an OAuth2
/// access token, optionally paired with a user id + nick) and commits them in a single
/// <c>POST /users/@me/channels</c> request on <see cref="ExecuteAsync"/>.
/// </summary>
internal sealed class CreateGroupDmAction : RestAction<IGroupDmChannel>, ICreateGroupDmAction
{
	/// <summary>Discord caps group DMs at 10 participants per the API docs.</summary>
	internal const int MaxRecipients = 10;

	private readonly DiscordClient _client;
	private readonly List<string> _accessTokens = [];
	private Dictionary<string, string>? _nicks;

	public CreateGroupDmAction(DiscordClient client)
	{
		_client = client ?? throw new ArgumentNullException(nameof(client));
	}

	/// <inheritdoc />
	public ICreateGroupDmAction AddRecipient(string accessToken)
	{
		EnsureValidAccessToken(accessToken);
		EnsureCapacity();
		_accessTokens.Add(accessToken);
		return this;
	}

	/// <inheritdoc />
	public ICreateGroupDmAction AddRecipient(string accessToken, Snowflake userId, string nick)
	{
		EnsureValidAccessToken(accessToken);
		if (userId == default)
			throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
		ArgumentException.ThrowIfNullOrWhiteSpace(nick);

		EnsureCapacity();
		_accessTokens.Add(accessToken);
		_nicks ??= [];
		_nicks[userId.ToString()] = nick;
		return this;
	}

	/// <inheritdoc />
	public override async Task<IGroupDmChannel> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		if (_accessTokens.Count < 2)
			throw new InvalidOperationException(
				$"A group DM requires at least 2 recipients. Currently registered: {_accessTokens.Count}. " +
				"Call AddRecipient for each user before ExecuteAsync.");

		var request = new CreateGroupDmRequest
		{
			AccessTokens = [.. _accessTokens],
			Nicks = _nicks,
		};

		var channel = await _client.ChannelClient.CreateGroupDmAsync(request, cancellationToken);
		return new GroupDmChannelWrapper(_client, channel);
	}

	private static void EnsureValidAccessToken(string accessToken)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);
	}

	private void EnsureCapacity()
	{
		if (_accessTokens.Count >= MaxRecipients)
			throw new InvalidOperationException(
				$"A group DM cannot have more than {MaxRecipients} recipients (Discord API limit).");
	}
}

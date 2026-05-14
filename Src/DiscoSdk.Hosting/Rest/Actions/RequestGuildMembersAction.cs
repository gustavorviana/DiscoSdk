using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Models;
using DiscoSdk.Models.Requests.Members;
using DiscoSdk.Rest.Actions;
using System.Runtime.CompilerServices;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="IRequestGuildMembersAction"/>. Both terminal methods share the
/// same op-8 send + nonce-correlated <see cref="IMemberChunkSink"/> registration; they only
/// differ in which sink they use (buffering vs streaming).
/// </summary>
internal sealed class RequestGuildMembersAction(DiscordClient client, Snowflake guildId)
	: IRequestGuildMembersAction
{
	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
	private readonly Snowflake _guildId = guildId;

	private string? _query;
	private int _limit;
	private bool? _presences;
	private Snowflake[]? _userIds;

	/// <inheritdoc />
	public IRequestGuildMembersAction SetQuery(string query)
	{
		_query = query ?? throw new ArgumentNullException(nameof(query));
		_userIds = null;
		return this;
	}

	/// <inheritdoc />
	public IRequestGuildMembersAction SetLimit(int limit)
	{
		if (limit < 0)
			throw new ArgumentOutOfRangeException(nameof(limit), "Limit cannot be negative.");
		_limit = limit;
		return this;
	}

	/// <inheritdoc />
	public IRequestGuildMembersAction SetPresences(bool presences)
	{
		_presences = presences;
		return this;
	}

	/// <inheritdoc />
	public IRequestGuildMembersAction SetUserIds(params Snowflake[] userIds)
	{
		ArgumentNullException.ThrowIfNull(userIds);
		if (userIds.Length > 100)
			throw new ArgumentException("user_ids cannot contain more than 100 ids.", nameof(userIds));
		_userIds = userIds;
		_query = null;
		return this;
	}

	/// <inheritdoc />
	public async Task<IReadOnlyList<IMember>> GetAsync(CancellationToken cancellationToken = default)
	{
		var nonce = Guid.NewGuid().ToString("N");
		var sink = new BufferingMemberChunkSink();
		_client.MemberChunkCoordinator.Register(nonce, sink);

		try
		{
			await SendRequestAsync(nonce, cancellationToken).ConfigureAwait(false);

			using (cancellationToken.Register(() => _client.MemberChunkCoordinator.Cancel(nonce, cancellationToken)))
				return await sink.Completion.ConfigureAwait(false);
		}
		catch
		{
			_client.MemberChunkCoordinator.Cancel(nonce, cancellationToken);
			throw;
		}
	}

	/// <inheritdoc />
	public async IAsyncEnumerable<IMember> StreamAsync(
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var nonce = Guid.NewGuid().ToString("N");
		var sink = new StreamingMemberChunkSink();
		_client.MemberChunkCoordinator.Register(nonce, sink);

		try
		{
			await SendRequestAsync(nonce, cancellationToken).ConfigureAwait(false);

			await foreach (var member in sink.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
				yield return member;
		}
		finally
		{
			// Idempotent — if the final chunk already removed the entry, this is a no-op.
			_client.MemberChunkCoordinator.Cancel(nonce, cancellationToken);
		}
	}

	private async Task SendRequestAsync(string nonce, CancellationToken cancellationToken)
	{
		// Discord requires either query OR user_ids. When neither is set, default to fetching
		// the whole guild via empty query (caller needs the GUILD_MEMBERS privileged intent).
		if (_query is null && _userIds is null)
			_query = string.Empty;

		var request = new RequestGuildMembersRequest
		{
			GuildId = _guildId.ToString(),
			Query = _query,
			Limit = _limit,
			Presences = _presences,
			UserIds = _userIds?.Select(id => id.ToString()).ToArray(),
			Nonce = nonce,
		};

		var shard = _client.GetShardForGuild(_guildId);
		await shard.SendAsync(Gateway.OpCodes.RequestGuildMembers, request, cancellationToken).ConfigureAwait(false);
	}
}

using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="ICreateInviteAction"/> for creating invites.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CreateInviteAction"/> class.
/// </remarks>
/// <param name="client">The Discord client.</param>
/// <param name="channelId">The ID of the channel to create an invite for.</param>
internal class CreateInviteAction(DiscordClient client, IGuildChannel channel) : RestAction<IInvite>, ICreateInviteAction
{
	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
	private int? _maxAge;
	private int? _maxUses;
	private bool? _temporary;
	private bool? _unique;
	private ulong? _targetApplicationId;
	private DiscordId? _targetStreamUserId;
	private Func<bool>? _check;

	/// <inheritdoc />
	public ICreateInviteAction SetCheck(Func<bool> check)
	{
		_check = check;
		return this;
	}

	/// <inheritdoc />
	public ICreateInviteAction SetMaxAge(int? seconds)
	{
		_maxAge = seconds;
		return this;
	}

	/// <inheritdoc />
	public ICreateInviteAction SetMaxAge(long? timeout, TimeSpan unit)
	{
		if (timeout.HasValue && unit != TimeSpan.Zero)
			_maxAge = (int)(timeout.Value * unit.TotalSeconds);
		return this;
	}

	/// <inheritdoc />
	public ICreateInviteAction SetMaxUses(int? maxUses)
	{
		_maxUses = maxUses;
		return this;
	}

	/// <inheritdoc />
	public ICreateInviteAction SetTemporary(bool? temporary)
	{
		_temporary = temporary;
		return this;
	}

	/// <inheritdoc />
	public ICreateInviteAction SetUnique(bool? unique)
	{
		_unique = unique;
		return this;
	}

	/// <inheritdoc />
	public ICreateInviteAction SetTargetApplication(ulong applicationId)
	{
		_targetApplicationId = applicationId;
		_targetStreamUserId = null;
		return this;
	}

	/// <inheritdoc />
	public ICreateInviteAction SetTargetApplication(string applicationId)
	{
		if (ulong.TryParse(applicationId, out var id))
		{
			_targetApplicationId = id;
			_targetStreamUserId = null;
		}
		return this;
	}

	/// <inheritdoc />
	public ICreateInviteAction SetTargetStream(ulong userId)
	{
		_targetStreamUserId = new DiscordId(userId);
		_targetApplicationId = null;
		return this;
	}

	/// <inheritdoc />
	public ICreateInviteAction SetTargetStream(string userId)
	{
		if (DiscordId.TryParse(userId, out var id))
		{
			_targetStreamUserId = id;
			_targetApplicationId = null;
		}
		return this;
	}

	/// <inheritdoc />
	public ICreateInviteAction SetTargetStream(IUser user)
	{
		_targetStreamUserId = user.Id;
		_targetApplicationId = null;
		return this;
	}

	/// <inheritdoc />
	public ICreateInviteAction SetTargetStream(IMember member)
	{
		_targetStreamUserId = member.Id;
		_targetApplicationId = null;
		return this;
	}

	/// <inheritdoc />
	public override async Task<IInvite> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		if (_check != null && !_check())
			throw new InvalidOperationException("Check function returned false.");

		var request = new Dictionary<string, object?>();

		if (_maxAge.HasValue)
			request["max_age"] = _maxAge.Value;

		if (_maxUses.HasValue)
			request["max_uses"] = _maxUses.Value;

		if (_temporary.HasValue)
			request["temporary"] = _temporary.Value;

		if (_unique.HasValue)
			request["unique"] = _unique.Value;

		if (_targetApplicationId.HasValue)
		{
			request["target_type"] = (int)InviteTargetType.EmbeddedApplication;
			request["target_application_id"] = _targetApplicationId.Value.ToString();
		}
		else if (_targetStreamUserId.HasValue)
		{
			request["target_type"] = (int)InviteTargetType.Stream;
			request["target_user_id"] = _targetStreamUserId.Value.ToString();
		}

		var invite = await _client.InviteClient.CreateAsync(channel.Id, request, cancellationToken);
		return new InviteWrapper(invite, channel, _client);
	}
}
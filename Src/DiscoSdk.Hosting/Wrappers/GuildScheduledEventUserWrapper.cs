using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Wraps a <see cref="GuildScheduledEventUser"/> raw payload, projecting <see cref="User"/> and (optional)
/// <see cref="GuildMember"/> into their public-surface wrappers.
/// </summary>
internal sealed class GuildScheduledEventUserWrapper : IGuildScheduledEventUser
{
	private readonly DiscordClient _client;
	private readonly GuildScheduledEventUser _model;
	private readonly IGuild? _guild;

	public GuildScheduledEventUserWrapper(DiscordClient client, GuildScheduledEventUser model, IGuild? guild)
	{
		_client = client;
		_model = model;
		_guild = guild;
	}

	/// <inheritdoc />
	public Snowflake ScheduledEventId => _model.GuildScheduledEventId;

	/// <inheritdoc />
	public IUser User => new UserWrapper(_client, _model.User);

	/// <inheritdoc />
	public IMember? Member => _model.Member is not null && _guild is not null
		? new GuildMemberWrapper(_client, _model.Member, _guild)
		: null;
}

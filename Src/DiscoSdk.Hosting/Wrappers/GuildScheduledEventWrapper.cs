using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Public wrapper over <see cref="GuildScheduledEvent"/> that exposes the REST actions
/// (Modify, Delete, GetUsers) on the typed surface.
/// </summary>
internal sealed class GuildScheduledEventWrapper(DiscordClient client, GuildScheduledEvent model) : IGuildScheduledEvent
{
	/// <inheritdoc />
	public Snowflake Id => model.Id;
	/// <inheritdoc />
	public Snowflake GuildId => model.GuildId;
	/// <inheritdoc />
	public Snowflake? ChannelId => model.ChannelId;
	/// <inheritdoc />
	public Snowflake? CreatorId => model.CreatorId;
	/// <inheritdoc />
	public IUser? Creator => model.Creator is null ? null : new UserWrapper(client, model.Creator);
	/// <inheritdoc />
	public string Name => model.Name;
	/// <inheritdoc />
	public string? Description => model.Description;
	/// <inheritdoc />
	public DateTimeOffset ScheduledStartTime => model.ScheduledStartTime;
	/// <inheritdoc />
	public DateTimeOffset? ScheduledEndTime => model.ScheduledEndTime;
	/// <inheritdoc />
	public ScheduledEventPrivacyLevel PrivacyLevel => model.PrivacyLevel;
	/// <inheritdoc />
	public ScheduledEventStatus Status => model.Status;
	/// <inheritdoc />
	public ScheduledEventEntityType EntityType => model.EntityType;
	/// <inheritdoc />
	public Snowflake? EntityId => model.EntityId;
	/// <inheritdoc />
	public ScheduledEventEntityMetadata? EntityMetadata => model.EntityMetadata;
	/// <inheritdoc />
	public int? UserCount => model.UserCount;
	/// <inheritdoc />
	public string? Image => model.Image;

	/// <inheritdoc />
	public IModifyScheduledEventAction Modify()
		=> new ModifyScheduledEventAction(client, model.GuildId, model.Id);

	/// <inheritdoc />
	public IRestAction Delete()
		=> RestAction.Create(ct => client.GuildScheduledEventClient.DeleteAsync(model.GuildId, model.Id, ct));

	/// <inheritdoc />
	public IRestAction<IReadOnlyList<IGuildScheduledEventUser>> GetUsers(
		int? limit = null,
		bool? withMember = null,
		Snowflake? before = null,
		Snowflake? after = null)
	{
		return RestAction<IReadOnlyList<IGuildScheduledEventUser>>.Create(async ct =>
		{
			var raw = await client.GuildScheduledEventClient.GetUsersAsync(model.GuildId, model.Id, limit, withMember, before, after, ct);

			var guild = client.Guilds.GetWrapped(model.GuildId);
			return raw
				.Select(u => (IGuildScheduledEventUser)new GuildScheduledEventUserWrapper(client, u, guild))
				.ToList()
				.AsReadOnly();
		});
	}
}

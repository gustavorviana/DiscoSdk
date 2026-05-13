using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;
using DiscoSdk.Hosting.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Wraps an <see cref="Integration"/> and exposes the operations available on it.
/// </summary>
internal sealed class IntegrationWrapper(DiscordClient client, Snowflake guildId, Integration model) : IIntegration
{
	private readonly Integration _model = model ?? throw new ArgumentNullException(nameof(model));
	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
	private IUser? _user;

	public Snowflake Id => _model.Id;
	public DateTimeOffset CreatedAt => Id.CreatedAt;
	public string Name => _model.Name;
	public string Type => _model.Type;
	public bool Enabled => _model.Enabled;
	public bool? Syncing => _model.Syncing;
	public Snowflake? RoleId => _model.RoleId;
	public bool? EnableEmoticons => _model.EnableEmoticons;
	public IntegrationExpireBehavior? ExpireBehavior => _model.ExpireBehavior;
	public int? ExpireGracePeriod => _model.ExpireGracePeriod;
	public IUser? User => _user ??= _model.User is { } u ? new UserWrapper(_client, u) : null;
	public IIntegrationAccount Account => _model.Account;
	public DateTimeOffset? SyncedAt => _model.SyncedAt;
	public int? SubscriberCount => _model.SubscriberCount;
	public bool? Revoked => _model.Revoked;
	public IReadOnlyList<string>? Scopes => _model.Scopes;

	public IRestAction Delete()
		=> RestAction.Create(ct => _client.GuildClient.DeleteIntegrationAsync(guildId, _model.Id, ct));
}

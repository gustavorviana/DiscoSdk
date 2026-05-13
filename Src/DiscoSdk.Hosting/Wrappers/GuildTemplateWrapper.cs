using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Wraps a <see cref="GuildTemplate"/> model and exposes the operations available on it.
/// </summary>
internal sealed class GuildTemplateWrapper(DiscordClient client, GuildTemplate model) : IGuildTemplate
{
	private readonly GuildTemplate _model = model ?? throw new ArgumentNullException(nameof(model));
	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
	private IUser? _creator;

	public string Code => _model.Code;
	public string Name => _model.Name;
	public string? Description => _model.Description;
	public int UsageCount => _model.UsageCount;
	public Snowflake CreatorId => _model.CreatorId;
	public IUser Creator => _creator ??= new UserWrapper(_client, _model.Creator);
	public string CreatedAt => _model.CreatedAt;
	public string UpdatedAt => _model.UpdatedAt;
	public Snowflake SourceGuildId => _model.SourceGuildId;
	public IGuild SerializedSourceGuild => new GuildWrapper(_model.SerializedSourceGuild, _client);
	public bool? IsDirty => _model.IsDirty;

	public IRestAction<IGuild> CreateGuild(string name, string? icon = null)
		=> RestAction<IGuild>.Create(async ct => new GuildWrapper(await _client.GuildTemplateClient.CreateGuildFromTemplateAsync(_model.Code, name, icon, ct), _client));

	public IRestAction<IGuildTemplate> Sync()
		=> RestAction<IGuildTemplate>.Create(async ct => new GuildTemplateWrapper(_client, await _client.GuildTemplateClient.SyncGuildTemplateAsync(_model.SourceGuildId, _model.Code, ct)));

	public IRestAction<IGuildTemplate> Modify(string? name = null, string? description = null)
		=> RestAction<IGuildTemplate>.Create(async ct => new GuildTemplateWrapper(_client, await _client.GuildTemplateClient.ModifyGuildTemplateAsync(_model.SourceGuildId, _model.Code, name, description, ct)));

	public IRestAction<IGuildTemplate> Delete()
		=> RestAction<IGuildTemplate>.Create(async ct => new GuildTemplateWrapper(_client, await _client.GuildTemplateClient.DeleteGuildTemplateAsync(_model.SourceGuildId, _model.Code, ct)));
}

using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Wraps a <see cref="Webhook"/> and exposes the operations available on it.
/// </summary>
internal sealed class WebhookWrapper(DiscordClient client, Webhook model) : IWebhook
{
	private readonly Webhook _model = model ?? throw new ArgumentNullException(nameof(model));
	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
	private IUser? _user;

	public Snowflake Id => _model.Id;
	public DateTimeOffset CreatedAt => Id.CreatedAt;
	public WebhookType Type => _model.Type;
	public Snowflake? GuildId => _model.GuildId;
	public Snowflake? ChannelId => _model.ChannelId;
	public IUser? User => _user ??= _model.User is { } u ? new UserWrapper(_client, u) : null;
	public string? Name => _model.Name;
	public string? Avatar => _model.Avatar;
	public string? Token => _model.Token;
	public Snowflake? ApplicationId => _model.ApplicationId;
	public string? Url => _model.Url;

	public IModifyWebhookAction Modify() => new ModifyWebhookAction(_client, _model.Id);

	public IRestAction Delete()
		=> RestAction.Create(ct => _client.WebhookClient.DeleteAsync(_model.Id, ct));
}

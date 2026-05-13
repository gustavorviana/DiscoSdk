using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models.Applications;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="IEditApplicationAction"/>.
/// </summary>
internal sealed class EditApplicationAction(DiscordClient client) : RestAction<IApplication>, IEditApplicationAction
{
	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
	private readonly Dictionary<string, object?> _changes = [];

	public IEditApplicationAction SetCustomInstallUrl(string? url) { _changes["custom_install_url"] = url; return this; }
	public IEditApplicationAction SetDescription(string description) { _changes["description"] = description ?? throw new ArgumentNullException(nameof(description)); return this; }
	public IEditApplicationAction SetRoleConnectionsVerificationUrl(string? url) { _changes["role_connections_verification_url"] = url; return this; }
	public IEditApplicationAction SetInstallParams(ApplicationInstallParams? installParams) { _changes["install_params"] = installParams; return this; }
	public IEditApplicationAction SetFlags(ApplicationFlags flags) { _changes["flags"] = (int)flags; return this; }
	public IEditApplicationAction SetIcon(string? icon) { _changes["icon"] = icon; return this; }
	public IEditApplicationAction SetCoverImage(string? coverImage) { _changes["cover_image"] = coverImage; return this; }
	public IEditApplicationAction SetInteractionsEndpointUrl(string? url) { _changes["interactions_endpoint_url"] = url; return this; }
	public IEditApplicationAction SetTags(params string[] tags) { _changes["tags"] = tags; return this; }
	public IEditApplicationAction SetEventWebhooksUrl(string? url) { _changes["event_webhooks_url"] = url; return this; }

	public override async Task<IApplication> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		var application = await _client.ApplicationClient.EditCurrentApplicationAsync(_changes, cancellationToken);
		return new ApplicationWrapper(_client, application);
	}
}

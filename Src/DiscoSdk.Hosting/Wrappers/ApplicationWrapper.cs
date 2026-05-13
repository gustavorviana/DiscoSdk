using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Models.Applications;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;
using System.Collections.Generic;
using System.Linq;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Wraps an <see cref="Application"/> model and exposes the operations available on it.
/// </summary>
internal sealed class ApplicationWrapper(DiscordClient client, Application model) : IApplication
{
	private readonly Application _model = model ?? throw new ArgumentNullException(nameof(model));
	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
	private IUser? _bot;
	private IUser? _owner;
	private ITeam? _team;
	private IReadOnlyDictionary<string, IApplicationIntegrationTypeConfiguration>? _integrationTypesConfig;

	public Snowflake Id => _model.Id;
	public string Name => _model.Name;
	public string? Icon => _model.Icon;
	public string Description => _model.Description;
	public IReadOnlyList<string>? RpcOrigins => _model.RpcOrigins;
	public bool BotPublic => _model.BotPublic;
	public bool BotRequireCodeGrant => _model.BotRequireCodeGrant;
	public IUser? Bot => _bot ??= _model.Bot is { } b ? new UserWrapper(_client, b) : null;
	public string? TermsOfServiceUrl => _model.TermsOfServiceUrl;
	public string? PrivacyPolicyUrl => _model.PrivacyPolicyUrl;
	public IUser? Owner => _owner ??= _model.Owner is { } o ? new UserWrapper(_client, o) : null;
	public string VerifyKey => _model.VerifyKey;
	public ITeam? Team => _team ??= _model.Team is { } t ? new TeamWrapper(_client, t) : null;
	public Snowflake? GuildId => _model.GuildId;
	public Snowflake? PrimarySkuId => _model.PrimarySkuId;
	public string? Slug => _model.Slug;
	public string? CoverImage => _model.CoverImage;
	public ApplicationFlags? Flags => _model.Flags;
	public int? ApproximateGuildCount => _model.ApproximateGuildCount;
	public int? ApproximateUserInstallCount => _model.ApproximateUserInstallCount;
	public IReadOnlyList<string>? RedirectUris => _model.RedirectUris;
	public string? InteractionsEndpointUrl => _model.InteractionsEndpointUrl;
	public string? RoleConnectionsVerificationUrl => _model.RoleConnectionsVerificationUrl;
	public string? EventWebhooksUrl => _model.EventWebhooksUrl;
	public IReadOnlyList<string>? Tags => _model.Tags;
	public IApplicationInstallParams? InstallParams => _model.InstallParams;
	public IReadOnlyDictionary<string, IApplicationIntegrationTypeConfiguration>? IntegrationTypesConfig =>
		_integrationTypesConfig ??= _model.IntegrationTypesConfig?.ToDictionary(kvp => kvp.Key, kvp => (IApplicationIntegrationTypeConfiguration)kvp.Value);
	public string? CustomInstallUrl => _model.CustomInstallUrl;

	public IEditApplicationAction Edit() => new EditApplicationAction(_client);
}

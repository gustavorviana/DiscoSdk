using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Monetization;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Wraps an <see cref="Entitlement"/> model and exposes the operations available on it.
/// </summary>
internal sealed class EntitlementWrapper(DiscordClient client, Entitlement model) : IEntitlement
{
	private readonly Entitlement _model = model ?? throw new ArgumentNullException(nameof(model));
	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));

	public Snowflake Id => _model.Id;
	public Snowflake SkuId => _model.SkuId;
	public Snowflake ApplicationId => _model.ApplicationId;
	public Snowflake? UserId => _model.UserId;
	public EntitlementType Type => _model.Type;
	public bool Deleted => _model.Deleted;
	public string? StartsAt => _model.StartsAt;
	public string? EndsAt => _model.EndsAt;
	public Snowflake? GuildId => _model.GuildId;
	public bool? Consumed => _model.Consumed;

	public IRestAction Consume()
		=> RestAction.Create(ct => _client.ApplicationClient.ConsumeEntitlementAsync(_model.ApplicationId, _model.Id, ct));

	public IRestAction DeleteTest()
		=> RestAction.Create(ct => _client.ApplicationClient.DeleteTestEntitlementAsync(_model.ApplicationId, _model.Id, ct));
}

using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Monetization;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Wraps a <see cref="Sku"/> model and exposes the operations available on it.
/// </summary>
internal sealed class SkuWrapper(DiscordClient client, Sku model) : ISku
{
	private readonly Sku _model = model ?? throw new ArgumentNullException(nameof(model));
	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));

	public Snowflake Id => _model.Id;
	public SkuType Type => _model.Type;
	public Snowflake ApplicationId => _model.ApplicationId;
	public string Name => _model.Name;
	public string Slug => _model.Slug;
	public SkuFlags Flags => _model.Flags;

	public IRestAction<IReadOnlyList<ISubscription>> GetSubscriptions(Snowflake? userId = null)
		=> RestAction<IReadOnlyList<ISubscription>>.Create(async ct => await _client.ApplicationClient.ListSkuSubscriptionsAsync(_model.Id, userId, ct));

	public IRestAction<ISubscription> GetSubscription(Snowflake subscriptionId)
		=> RestAction<ISubscription>.Create(async ct => await _client.ApplicationClient.GetSkuSubscriptionAsync(_model.Id, subscriptionId, ct));
}

using DiscoSdk.Models;
using DiscoSdk.Models.Activities;
using DiscoSdk.Models.Presences;

namespace DiscoSdk.Hosting.Wrappers;

internal sealed class PresenceWrapper(Presence model) : IPresence
{
	private IClientStatus? _clientStatus;

	public Snowflake UserId => model.User?.Id ?? default;
	public string? Status => model.Status;
	public long ProcessedAtTimestamp => model.ProcessedAtTimestamp;
	public Activity? Game => model.Game;
	public IClientStatus? ClientStatus => model.ClientStatus is null
		? null
		: _clientStatus ??= new ClientStatusWrapper(model.ClientStatus);
	public Activity[] Activities => model.Activities;
}

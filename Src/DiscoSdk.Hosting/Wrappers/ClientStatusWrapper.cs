using DiscoSdk.Models.Presences;

namespace DiscoSdk.Hosting.Wrappers;

internal sealed class ClientStatusWrapper(ClientStatus model) : IClientStatus
{
	public string? Desktop => model.Desktop;
	public string? Mobile => model.Mobile;
	public string? Web => model.Web;
}

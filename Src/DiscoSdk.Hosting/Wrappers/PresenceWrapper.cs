using DiscoSdk.Hosting.Wrappers.Activities;
using DiscoSdk.Models;
using DiscoSdk.Models.Activities;
using DiscoSdk.Models.Presences;

namespace DiscoSdk.Hosting.Wrappers;

internal sealed class PresenceWrapper(Presence model) : IPresence
{
	private IClientStatus? _clientStatus;
	private IActivity? _game;
	private IActivity[]? _activities;

	public Snowflake UserId => model.User?.Id ?? default;
	public string? Status => model.Status;
	public long ProcessedAtTimestamp => model.ProcessedAtTimestamp;

	public IActivity? Game
	{
		get
		{
			if (model.Game is null)
				return null;
			return _game ??= new ActivityWrapper(model.Game);
		}
	}

	public IClientStatus? ClientStatus => model.ClientStatus is null
		? null
		: _clientStatus ??= new ClientStatusWrapper(model.ClientStatus);

	public IActivity[] Activities
	{
		get
		{
			if (_activities is not null)
				return _activities;

			var raw = model.Activities;
			if (raw is null || raw.Length == 0)
				return _activities = [];

			var wrapped = new IActivity[raw.Length];
			for (var i = 0; i < raw.Length; i++)
				wrapped[i] = new ActivityWrapper(raw[i]);
			return _activities = wrapped;
		}
	}
}

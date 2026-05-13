using DiscoSdk.Models;
using DiscoSdk.Models.Applications;

namespace DiscoSdk.Hosting.Wrappers;

internal sealed class TeamWrapper(DiscordClient client, Team model) : ITeam
{
	private readonly Team _model = model ?? throw new ArgumentNullException(nameof(model));
	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
	private IReadOnlyList<ITeamMember>? _members;

	public Snowflake Id => _model.Id;
	public string? Icon => _model.Icon;
	public string Name => _model.Name;
	public Snowflake OwnerUserId => _model.OwnerUserId;
	public IReadOnlyList<ITeamMember> Members => _members ??=
		_model.Members.Select(m => (ITeamMember)new TeamMemberWrapper(_client, m)).ToList().AsReadOnly();
}

using DiscoSdk.Models;
using DiscoSdk.Models.Applications;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Hosting.Wrappers;

internal sealed class TeamMemberWrapper(DiscordClient client, TeamMember model) : ITeamMember
{
	private readonly TeamMember _model = model ?? throw new ArgumentNullException(nameof(model));
	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
	private IUser? _user;

	public TeamMembershipState MembershipState => _model.MembershipState;
	public Snowflake TeamId => _model.TeamId;
	public IUser User => _user ??= new UserWrapper(_client, _model.User);
	public string Role => _model.Role;
}

using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

internal sealed class AddMemberAction : RestAction<IMember?>, IAddMemberAction
{
    private readonly DiscordClient _client;
    private readonly GuildWrapper _guild;
    private readonly Snowflake _userId;
    private readonly string _accessToken;
    private string? _nick;
    private IEnumerable<Snowflake>? _roles;
    private bool? _mute;
    private bool? _deaf;

    public AddMemberAction(DiscordClient client, GuildWrapper guild, Snowflake userId, string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            throw new ArgumentException("Access token cannot be null or empty.", nameof(accessToken));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _guild = guild ?? throw new ArgumentNullException(nameof(guild));
        _userId = userId;
        _accessToken = accessToken;
    }

    public IAddMemberAction SetNickname(string? nick) { _nick = nick; return this; }
    public IAddMemberAction SetRoles(IEnumerable<Snowflake>? roles) { _roles = roles; return this; }
    public IAddMemberAction SetMuted(bool? muted) { _mute = muted; return this; }
    public IAddMemberAction SetDeafened(bool? deafened) { _deaf = deafened; return this; }

    public override async Task<IMember?> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var member = await _client.GuildClient.AddMemberAsync(
            _guild.Id, _userId, _accessToken, _nick, _roles, _mute, _deaf, cancellationToken);
        return member is null ? null : new GuildMemberWrapper(_client, member, _guild);
    }
}

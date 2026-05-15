using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

internal sealed class ModifyMemberAction : RestAction<IMember>, IModifyMemberAction
{
    private readonly DiscordClient _client;
    private readonly GuildWrapper _guild;
    private readonly Snowflake _userId;
    private readonly Dictionary<string, object?> _body = new();

    public ModifyMemberAction(DiscordClient client, GuildWrapper guild, Snowflake userId)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _guild = guild ?? throw new ArgumentNullException(nameof(guild));
        _userId = userId;
    }

    public IModifyMemberAction SetNickname(string? nick)
    {
        _body["nick"] = nick;
        return this;
    }

    public IModifyMemberAction SetRoles(IEnumerable<Snowflake> roles)
    {
        ArgumentNullException.ThrowIfNull(roles);
        _body["roles"] = roles.Select(r => r.ToString()).ToArray();
        return this;
    }

    public IModifyMemberAction SetMuted(bool muted)
    {
        _body["mute"] = muted;
        return this;
    }

    public IModifyMemberAction SetDeafened(bool deafened)
    {
        _body["deaf"] = deafened;
        return this;
    }

    public IModifyMemberAction MoveToVoiceChannel(Snowflake? channelId)
    {
        _body["channel_id"] = channelId?.ToString();
        return this;
    }

    public IModifyMemberAction Timeout(DateTimeOffset until)
    {
        _body["communication_disabled_until"] = until.ToString("o");
        return this;
    }

    public IModifyMemberAction ClearTimeout()
    {
        _body["communication_disabled_until"] = null;
        return this;
    }

    public IModifyMemberAction SetFlags(int flags)
    {
        _body["flags"] = flags;
        return this;
    }

    public override async Task<IMember> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var member = await _client.GuildClient.ModifyMemberAsync(_guild.Id, _userId, _body, cancellationToken);
        return new GuildMemberWrapper(_client, member, _guild);
    }
}

using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

internal sealed class ModifyMeAction : RestAction<IUser>, IModifyMeAction
{
    private readonly DiscordClient _client;
    private readonly Dictionary<string, object?> _body = new();

    public ModifyMeAction(DiscordClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public IModifyMeAction SetUsername(string username)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        _body["username"] = username;
        return this;
    }

    public IModifyMeAction SetAvatar(string avatarDataUri)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(avatarDataUri);
        _body["avatar"] = avatarDataUri;
        return this;
    }

    public IModifyMeAction ClearAvatar()
    {
        _body["avatar"] = null;
        return this;
    }

    public IModifyMeAction SetBanner(string bannerDataUri)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bannerDataUri);
        _body["banner"] = bannerDataUri;
        return this;
    }

    public IModifyMeAction ClearBanner()
    {
        _body["banner"] = null;
        return this;
    }

    public override async Task<IUser> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var user = await _client.UserClient.ModifyCurrentAsync(_body, cancellationToken);
        return new UserWrapper(_client, user);
    }
}

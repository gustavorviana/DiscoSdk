using DiscoSdk.Models;
using DiscoSdk.Models.Applications;
using DiscoSdk.Models.OAuth2;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Wraps a <see cref="CurrentAuthorizationInfo"/> response and exposes the embedded application
/// + user through the existing <see cref="IApplication"/> / <see cref="IUser"/> wrappers.
/// </summary>
internal sealed class CurrentAuthorizationInfoWrapper(DiscordClient client, CurrentAuthorizationInfo info) : ICurrentAuthorizationInfo
{
    private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly CurrentAuthorizationInfo _info = info ?? throw new ArgumentNullException(nameof(info));
    private IApplication? _application;
    private IUser? _user;

    /// <inheritdoc />
    public IApplication Application => _application ??= new ApplicationWrapper(_client, _info.Application);

    /// <inheritdoc />
    public IReadOnlyList<string> Scopes => _info.Scopes;

    /// <inheritdoc />
    public DateTimeOffset Expires => _info.Expires;

    /// <inheritdoc />
    public IUser? User => _user ??= _info.User is null ? null : new UserWrapper(_client, _info.User);
}

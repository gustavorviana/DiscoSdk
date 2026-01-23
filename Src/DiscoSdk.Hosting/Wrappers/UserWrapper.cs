using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Users;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Wrapper that implements <see cref="IUser"/> for a <see cref="User"/> instance.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UserWrapper"/> class.
/// </remarks>
/// <param name="user">The user instance to wrap.</param>
/// <param name="client">The Discord client for performing operations.</param>
internal class UserWrapper : IUser
{
    private User _user;
    private readonly DiscordClient _client;

    public UserWrapper(DiscordClient client, User user)
    {
        _user = user ?? throw new ArgumentNullException(nameof(user));
        _client = client ?? throw new ArgumentNullException(nameof(client));

        Avatar = DiscordImageUrl.ParseAvatar(_user.UserId, user.Avatar);
        Banner = DiscordImageUrl.ParseBanner(_user.UserId, _user.Banner);
    }

    /// <inheritdoc />
    public Snowflake Id => _user.UserId;

    /// <inheritdoc />
    public DateTimeOffset CreatedAt => _user.UserId.CreatedAt;

    /// <inheritdoc />
    public string Username => _user.Username;

    /// <inheritdoc />
    public string Discriminator => _user.Discriminator;

    /// <inheritdoc />
    public string? GlobalName => _user.GlobalName;

    /// <inheritdoc />
    public DiscordImageUrl? Avatar { get; private set; }

    /// <inheritdoc />
    public bool Bot => _user.Bot ?? false;

    /// <inheritdoc />
    public bool System => _user.System ?? false;

    /// <inheritdoc />
    public bool MfaEnabled => _user.MfaEnabled ?? false;

    /// <inheritdoc />
    public DiscordImageUrl? Banner { get; private set; }

    /// <inheritdoc />
    public int? AccentColor => _user.AccentColor;

    /// <inheritdoc />
    public string? Locale => _user.Locale;

    /// <inheritdoc />
    public bool Verified => _user.Verified ?? false;

    /// <inheritdoc />
    public string? Email => _user.Email;

    /// <inheritdoc />
    public UserFlags Flags => _user.Flags;

    /// <inheritdoc />
    public PremiumType PremiumType => _user.PremiumType;

    /// <inheritdoc />
    public UserFlags PublicFlags => _user.PublicFlags;

    /// <inheritdoc />
    public string EffectiveAvatarUrl
    {
        get
        {
            if (!string.IsNullOrEmpty(_user.Avatar))
            {
                var extension = _user.Avatar.StartsWith("a_") ? "gif" : "png";
                return $"https://cdn.discordapp.com/avatars/{_user.UserId}/{_user.Avatar}.{extension}";
            }

            // Default avatar based on discriminator
            var discriminator = int.TryParse(_user.Discriminator, out var disc) ? disc : 0;
            return $"https://cdn.discordapp.com/embed/avatars/{discriminator % 5}.png";
        }
    }

    /// <inheritdoc />
    public string? EffectiveBannerUrl
    {
        get
        {
            if (string.IsNullOrEmpty(_user.Banner))
                return null;

            var extension = _user.Banner.StartsWith("a_") ? "gif" : "png";
            return $"https://cdn.discordapp.com/banners/{_user.UserId}/{_user.Banner}.{extension}";
        }
    }

    /// <inheritdoc />
    public string DisplayName => _user.GlobalName ?? _user.Username;

    /// <inheritdoc />
    public IRestAction<IDmChannel> OpenPrivateChannel()
    {
        _client.Users.Add(this);
        return _client.DmRepository.OpenDm(Id);
    }

    internal void OnUpdate(User user)
    {
        _user = user;
        Avatar = DiscordImageUrl.ParseAvatar(_user.UserId, _user.Avatar);
        Banner = DiscordImageUrl.ParseBanner(_user.UserId, _user.Banner);
    }
}
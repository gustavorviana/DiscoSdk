using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Mentions;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers.Messages;

internal class UserMentionWrapper(DiscordClient client, MessageMentionUser user, IGuild? guild) : IUserMention
{
    public Snowflake UserId => user.UserId;

    public string Username => user.Username;

    public string? GlobalName => user.GlobalName;

    public DiscordImageUrl? Avatar { get; } = DiscordImageUrl.ParseAvatar(user.UserId, user.Avatar);

    public DiscordImageUrl? Banner { get; } = DiscordImageUrl.ParseBanner(user.UserId, user.Banner);

    public UserFlags PublicFlags => user.PublicFlags;

    public UserFlags Flags => user.Flags;

    public IGuild? Guild => guild;

    public IRestAction<IMember?> ResolveMemberAsync()
    {
        if (guild == null)
            return RestAction<IMember>.Empty;

        return guild.GetMember(UserId);
    }

    public IRestAction<IUser> ResolveUserAsync()
    {
        return client.Users.Get(UserId)!;
    }
}
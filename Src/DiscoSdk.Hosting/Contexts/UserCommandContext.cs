using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Interactions;
using DiscoSdk.Models.Users;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Contexts;

internal class UserCommandContext : InteractionContextWrapper, IUserCommandContext
{
    public string Name { get; }
    public IUser TargetUser { get; }
    public IMember? TargetMember { get; }
    public IRestAction<IInteractionResolved?> Resolved { get; }

    public UserCommandContext(DiscordClient client, InteractionWrapper interaction)
        : base(client, interaction)
    {
        Name = interaction.Data?.Name ?? string.Empty;
        Resolved = interaction.Data?.GetResolved() ?? RestAction<IInteractionResolved?>.Empty;

        var rawData = interaction.RawInteraction.Data;
        var (user, member) = ResolveTarget(rawData, interaction.Guild);

        TargetUser = user is not null
            ? new UserWrapper(client, user)
            : throw new InvalidOperationException("User command context requires a target user in resolved data.");

        if (member is not null && interaction.Guild is not null)
            TargetMember = new GuildMemberWrapper(client, member, interaction.Guild);
    }

    internal static (User? user, GuildMember? member) ResolveTarget(
        InteractionData? data, IGuild? guild)
    {
        var targetId = data?.TargetId?.ToString();
        if (targetId is null)
            return (null, null);

        User? user = null;
        data?.Resolved?.Users?.TryGetValue(targetId, out user);

        GuildMember? member = null;
        if (guild is not null)
        {
            data?.Resolved?.Members?.TryGetValue(targetId, out member);
            // Discord omits the User field from resolved members; merge it
            if (member is not null)
                member.User ??= user;
        }

        return (user, member);
    }
}

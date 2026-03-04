using DiscoSdk.Commands;
using DiscoSdk.Contexts.Interactions;

namespace TomoriBot.Commands;

/// <summary>
/// Exemplos de comandos de contexto de usuário (User Context Menu).
/// </summary>
public class UserContextCommands : UserContextMenuHandler
{
    [UserCommand("show_user_info", GuildIds = ["773618860875579422"])]
    public async Task ShowUserInfoAsync(IUserCommandContext context)
    {
        var user = context.TargetUser;
        var member = context.TargetMember;

        var lines = new List<string>
        {
            $"**User:** {user.DisplayName} (`{user.Username}#{user.Discriminator}`)",
            $"**Bot:** {(user.Bot ? "Yes" : "No")}"
        };

        if (member != null)
        {
            if (!string.IsNullOrWhiteSpace(member.Nickname))
                lines.Add($"**Nickname:** {member.Nickname}");

            lines.Add($"**Guild:** {member.Guild.Name}");
        }

        await context
            .Reply(string.Join("\n", lines))
            .SetEphemeral()
            .ExecuteAsync();
    }

    [UserCommand("dm_user_ping", GuildIds = ["773618860875579422"])]
    public Task PingUserAsync(IUserCommandContext context)
    {
        var user = context.TargetUser;

        return context
            .Reply($"Test ping for **{user.DisplayName}** received successfully (no real DM was sent).")
            .SetEphemeral()
            .ExecuteAsync();
    }
}


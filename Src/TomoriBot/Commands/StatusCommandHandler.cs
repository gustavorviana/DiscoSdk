using DiscoSdk.Commands;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Models.Enums;

namespace TomoriBot.Commands;

public class StatusCommandHandler : SlashCommandHandler
{
    [SlashCommand("status", "Update bot status.", GuildIds = ["773618860875579422"])]
    [SlashOption(SlashCommandOptionType.String, "status", "New bot status.", required: true)]
    [EnumChoices<OnlineStatus>(OptionName = "status", ExceptValues = [OnlineStatus.Offline])]
    protected async Task OnExecuteAsync(ICommandContext context)
    {
        var status = context.GetOption<OnlineStatus>("status");
        if (status == null)
        {
            await context
                .Reply("Invalid option")
                .SetEphemeral()
                .ExecuteAsync();
            return;
        }

        await context
            .Client
            .UpdatePresence()
            .SetStatus(status.Value)
            .ExecuteAsync();

        await context.Reply("Ok").SetEphemeral().ExecuteAsync();
    }
}

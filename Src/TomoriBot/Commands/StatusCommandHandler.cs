using DiscoSdk.Commands;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Models.Enums;
using TomoriBot.Autocompletes;

namespace TomoriBot.Commands;

[SlashCommand("status", "Update bot status.", GuildIds = ["773618860875579422"])]
[Option(ApplicationCommandOptionType.String, "status", "New bot status.", required: true, AutocompleteType = typeof(OnlineStatusAutocomplete))]
public class StatusCommandHandler : SlashCommandHandler
{
    protected override async Task OnExecuteAsync(ICommandContext context)
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

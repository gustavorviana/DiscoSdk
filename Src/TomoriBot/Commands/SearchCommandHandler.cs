using DiscoSdk.Commands;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;

namespace TomoriBot.Commands;

public class SearchCommandHandler : SlashCommandHandler
{
    [SlashCommand("search", "Search something.", GuildIds = ["773618860875579422"])]
    [SlashOption(SlashCommandOptionType.String, "query", "What to search for.", required: true, MinLength = 1, MaxLength = 100)]
    [SlashOption(SlashCommandOptionType.Integer, "limit", "Max results.", required: false, MinValue = 1, MaxValue = 25)]
    protected async Task OnExecuteAsync(ICommandContext context)
    {
        var query = context.GetOption<string>("query");
        await context
            .Reply(string.IsNullOrEmpty(query)
                ? "No search term provided."
                : $"You selected: **{query}**")
            .SetEphemeral()
            .ExecuteAsync();
    }

    [AutocompleteHandler("search", "query")]
    protected Task QueryAutocompleteAsync(IAutocompleteContext context)
    {
        return context.ReplyWithChoices([
            new SlashCommandOptionChoice{
                Name = "example 1",
                Value = "example 1"
            },
            new SlashCommandOptionChoice{
                Name = "example 2",
                Value = "example 2"
            }
        ]).ExecuteAsync();
    }
}

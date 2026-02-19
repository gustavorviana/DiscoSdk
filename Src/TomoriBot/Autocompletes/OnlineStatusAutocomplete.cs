using DiscoSdk.Commands;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Models.Enums;

namespace TomoriBot.Autocompletes;

public class OnlineStatusAutocomplete : IAutocomplete
{
    public Task ExecuteAsync(IAutocompleteContext context)
    {
        var value = context.FocusedOption.Value?.ToString() ?? "";
        var enums = Enum.GetNames(typeof(OnlineStatus)).Where(x => x.Contains(value, StringComparison.OrdinalIgnoreCase));

        return context.ReplyWithChoices(enums.Select(x => new DiscoSdk.Models.Commands.SlashCommandOptionChoice
        {
            Name = x,
            Value = x
        })).ExecuteAsync();
    }
}

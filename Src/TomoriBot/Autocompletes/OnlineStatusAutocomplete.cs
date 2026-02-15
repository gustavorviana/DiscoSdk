using DiscoSdk.Commands;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Models.Enums;

namespace TomoriBot.Autocompletes;

public class OnlineStatusAutocomplete : IAutocompleteHandler
{
    public Task CompleteAsync(IAutocompleteContext context)
    {
        var value = context.FocusedOption.Value?.ToString() ?? "";
        var enums = Enum.GetNames(typeof(OnlineStatus)).Where(x => x.Contains(value, StringComparison.OrdinalIgnoreCase));

        return context.ReplyWithChoices(enums.Select(x => new DiscoSdk.Models.Commands.ApplicationCommandOptionChoice
        {
            Name = x,
            Value = x
        })).ExecuteAsync();
    }
}

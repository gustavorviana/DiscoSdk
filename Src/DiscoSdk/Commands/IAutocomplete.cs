using DiscoSdk.Contexts.Interactions;

namespace DiscoSdk.Commands;

public interface IAutocomplete
{
    Task ExecuteAsync(IAutocompleteContext context);
}